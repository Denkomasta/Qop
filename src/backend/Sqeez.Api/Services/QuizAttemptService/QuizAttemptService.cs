using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.QuizSystem;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Services
{
    public class QuizAttemptService : BaseService<QuizAttemptService>, IQuizAttemptService
    {
        private readonly IBadgeService _badgeService;

        public QuizAttemptService(SqeezDbContext context, ILogger<QuizAttemptService> logger, IBadgeService badgeService)
            : base(context, logger)
        {
            _badgeService = badgeService;
        }

        private async Task<long?> GetNextQuestionId(long quizId, long quizQuestionId)
        {
            return await _context.QuizQuestions
                .Where(q => q.QuizId == quizId && q.Id > quizQuestionId)
                .OrderBy(q => q.Id)
                .Select(q => (long?)q.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<ServiceResult<QuizAttemptDto>> StartAttemptAsync(long studentId, StartQuizAttemptDto dto)
        {
            // Verify the student is actually enrolled in the subject this quiz belongs to
            var enrollment = await _context.Enrollments
                .Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId && e.StudentId == studentId);

            if (enrollment == null)
                return ServiceResult<QuizAttemptDto>.Failure("You are not enrolled in this subject.", ServiceError.Forbidden);

            // Verify the quiz exists and belongs to this subject
            var quiz = await _context.Quizzes.FindAsync(dto.QuizId);
            if (quiz == null || quiz.SubjectId != enrollment.SubjectId)
                return ServiceResult<QuizAttemptDto>.Failure("Quiz not found.", ServiceError.NotFound);

            // Max Retries Business Rule
            if (quiz.MaxRetries > 0)
            {
                var previousAttempts = await _context.QuizAttempts
                    .CountAsync(a => a.QuizId == dto.QuizId && a.EnrollmentId == dto.EnrollmentId);

                if (previousAttempts >= quiz.MaxRetries)
                    return ServiceResult<QuizAttemptDto>.Failure($"You have reached the maximum of {quiz.MaxRetries} retries for this quiz.", ServiceError.Conflict);
            }

            // Create the new Attempt
            var attempt = new QuizAttempt
            {
                QuizId = dto.QuizId,
                EnrollmentId = dto.EnrollmentId,
                StartTime = DateTime.UtcNow,
                Status = AttemptStatus.Created,
                TotalScore = 0
            };

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            long? nextQuestionId = await GetNextQuestionId(attempt.QuizId, 0);  // first question, biggest nonexisting id.

            return ServiceResult<QuizAttemptDto>.Ok(new QuizAttemptDto(
                attempt.Id, attempt.QuizId, attempt.EnrollmentId, attempt.StartTime,
                attempt.EndTime, attempt.Status, attempt.TotalScore, attempt.Mark, nextQuestionId));
        }

        public async Task<ServiceResult<QuestionAnsweredDto>> SubmitAnswerAsync(long attemptId, long studentId, SubmitQuestionResponseDto dto)
        {
            var attempt = await _context.QuizAttempts
                .Include(a => a.Enrollment)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return ServiceResult<QuestionAnsweredDto>.Failure("Attempt not found.", ServiceError.NotFound);
            if (attempt.Enrollment.StudentId != studentId) return ServiceResult<QuestionAnsweredDto>.Failure("Access denied.", ServiceError.Forbidden);

            if (attempt.Status == AttemptStatus.Created) attempt.Status = AttemptStatus.Started;
            if (attempt.Status != AttemptStatus.Started) return ServiceResult<QuestionAnsweredDto>.Failure("This quiz attempt is no longer in progress.", ServiceError.Conflict);

            var question = await _context.QuizQuestions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == dto.QuizQuestionId && q.QuizId == attempt.QuizId);

            if (question == null) return ServiceResult<QuestionAnsweredDto>.Failure("Question not found on this quiz.", ServiceError.NotFound);

            var response = await _context.QuizQuestionResponses
                .Include(r => r.Options)
                .FirstOrDefaultAsync(r => r.QuizAttemptId == attemptId && r.QuizQuestionId == dto.QuizQuestionId);

            if (response != null)
            {
                return ServiceResult<QuestionAnsweredDto>.Failure("You have already submitted an answer for this question and cannot change it.", ServiceError.Conflict);
            }

            response = new QuizQuestionResponse
            {
                QuizAttemptId = attemptId,
                QuizQuestionId = dto.QuizQuestionId,
                ResponseTimeMs = dto.ResponseTimeMs,
                FreeTextAnswer = dto.FreeTextAnswer
            };
            _context.QuizQuestionResponses.Add(response);

            if (dto.SelectedOptionIds != null && dto.SelectedOptionIds.Any())
            {
                var validOptions = question.Options.Where(o => dto.SelectedOptionIds.Contains(o.Id)).ToList();
                foreach (var opt in validOptions)
                {
                    response.Options.Add(opt);
                }
            }

            response.Score = 0;

            if (string.IsNullOrWhiteSpace(dto.FreeTextAnswer) && response.Options.Any())
            {
                var correctOptionIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList();
                var selectedIds = response.Options.Select(o => o.Id).ToList();

                bool isPerfectMatch = correctOptionIds.Count == selectedIds.Count && !correctOptionIds.Except(selectedIds).Any();

                if (isPerfectMatch)
                {
                    response.Score = question.Difficulty;
                }
            }

            await _context.SaveChangesAsync();

            var actualCorrectOptionIds = question.Options.Where(o => o.IsCorrect && !o.IsFreeText).Select(o => o.Id).ToList();
            var actualCorrectFreeText = question.Options.FirstOrDefault(o => o.IsCorrect && o.IsFreeText)?.Text;

            long? nextQuestionId = await GetNextQuestionId(attempt.QuizId, dto.QuizQuestionId);

            return ServiceResult<QuestionAnsweredDto>.Ok(new QuestionAnsweredDto(
                response.Id, response.QuizQuestionId, response.ResponseTimeMs,
                response.FreeTextAnswer, response.IsLiked, response.Score,
                response.Options.Select(o => o.Id).ToList(),
                actualCorrectOptionIds, actualCorrectFreeText,
                nextQuestionId
            ));
        }

        public async Task<ServiceResult<long?>> GetNextPendingQuestionIdAsync(long attemptId, long studentId)
        {
            var attempt = await _context.QuizAttempts
                .Include(a => a.Enrollment)
                .Include(a => a.Responses)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return ServiceResult<long?>.Failure("Attempt not found.", ServiceError.NotFound);
            if (attempt.Enrollment.StudentId != studentId) return ServiceResult<long?>.Failure("Access denied.", ServiceError.Forbidden);

            if (attempt.Status != AttemptStatus.Started && attempt.Status != AttemptStatus.Created)
                return ServiceResult<long?>.Failure("This attempt is no longer in progress.", ServiceError.Conflict);

            long lastAnsweredQuestionId = attempt.Responses.Any()
                ? attempt.Responses.Max(r => r.QuizQuestionId)
                : 0;

            long? nextQuestionId = await GetNextQuestionId(attempt.QuizId, lastAnsweredQuestionId);

            return ServiceResult<long?>.Ok(nextQuestionId);
        }

        public async Task<ServiceResult<QuizAttemptDto>> CompleteAttemptAsync(long attemptId, long studentId)
        {
            // Fetch the attempt and responses so we can calculate the final score.
            var attempt = await _context.QuizAttempts
                .Include(a => a.Enrollment)
                .Include(a => a.Responses)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return ServiceResult<QuizAttemptDto>.Failure("Attempt not found.", ServiceError.NotFound);
            if (attempt.Enrollment.StudentId != studentId) return ServiceResult<QuizAttemptDto>.Failure("Access denied.", ServiceError.Forbidden);
            if (attempt.Status != AttemptStatus.Started) return ServiceResult<QuizAttemptDto>.Failure("This attempt is already completed.", ServiceError.Conflict);

            attempt.Status = AttemptStatus.Completed;
            attempt.EndTime = DateTime.UtcNow;
            attempt.TotalScore = attempt.Responses.Sum(r => r.Score);

            // Find their highest score from previous completed attempts of this quiz
            int previousHighScore = await _context.QuizAttempts
                .Where(a => a.QuizId == attempt.QuizId &&
                            a.Enrollment.StudentId == studentId &&
                            a.Status == AttemptStatus.Completed &&
                            a.Id != attemptId)
                .Select(a => (int?)a.TotalScore)
                .MaxAsync() ?? 0;

            // Calculate the difference. If they scored lower than their best, this results in 0.
            int xpToAward = Math.Max(0, attempt.TotalScore - previousHighScore);

            // Only hit the database to update the student if they actually earned new XP
            if (xpToAward > 0)
            {
                var student = await _context.Students.FindAsync(studentId);
                if (student != null)
                {
                    student.CurrentXP += xpToAward;
                }
            }

            await _context.SaveChangesAsync();

            int maxPossibleScore = await _context.QuizQuestions
                .Where(q => q.QuizId == attempt.QuizId)
                .SumAsync(q => q.Difficulty);

            decimal scorePercentage = maxPossibleScore > 0 ? ((decimal)attempt.TotalScore / maxPossibleScore) * 100 : 0;

            // Calculate Total Attempts
            int totalAttempts = await _context.QuizAttempts
                .CountAsync(a => a.QuizId == attempt.QuizId &&
                                 a.Enrollment.StudentId == studentId &&
                                 a.Status == AttemptStatus.Completed);

            int perfectAnswersCount = attempt.Responses.Count(r => r.Score > 0);

            var metrics = new BadgeEvaluationMetrics(
                ScorePercentage: scorePercentage,
                TotalScore: attempt.TotalScore,
                PerfectAnswersCount: perfectAnswersCount,
                TotalAttempts: totalAttempts
            );

            var newBadges = await _badgeService.EvaluateAndAwardBadgesAsync(studentId, metrics);

            if (!newBadges.Success)
            {
                return ServiceResult<QuizAttemptDto>.Failure(newBadges.ErrorMessage ?? "Badge evaluation failed", ServiceError.InternalError);
            }

            return ServiceResult<QuizAttemptDto>.Ok(new QuizAttemptDto(
                attempt.Id, attempt.QuizId, attempt.EnrollmentId, attempt.StartTime,
                attempt.EndTime, attempt.Status, attempt.TotalScore, attempt.Mark, EarnedBadges: newBadges.Data));
        }

        public async Task<ServiceResult<QuizAttemptDetailDto>> GetAttemptDetailsAsync(long attemptId, long currentUserId, string currentUserRole)
        {
            var attempt = await _context.QuizAttempts
                .Include(a => a.Enrollment)
                .Include(a => a.Responses)
                    .ThenInclude(r => r.Options)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return ServiceResult<QuizAttemptDetailDto>.Failure("Attempt not found.", ServiceError.NotFound);

            // Students can only see their own. Teachers/Admins can see any.
            if (currentUserRole == "Student" && attempt.Enrollment.StudentId != currentUserId)
            {
                return ServiceResult<QuizAttemptDetailDto>.Failure("You can only view your own attempts.", ServiceError.Forbidden);
            }

            var responseDtos = attempt.Responses.Select(r => new QuestionResponseDto(
                r.Id, r.QuizQuestionId, r.ResponseTimeMs, r.FreeTextAnswer, r.IsLiked, r.Score,
                r.Options.Select(o => o.Id).ToList()
            )).ToList();

            return ServiceResult<QuizAttemptDetailDto>.Ok(new QuizAttemptDetailDto(
                attempt.Id, attempt.QuizId, attempt.EnrollmentId, attempt.StartTime,
                attempt.EndTime, attempt.Status, attempt.TotalScore, attempt.Mark, responseDtos));
        }

        public async Task<ServiceResult<PagedResponse<QuizAttemptDto>>> GetAttemptsForQuizAsync(long quizId, long userId, int pageNumber = 1, int pageSize = 20)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Subject)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                return ServiceResult<PagedResponse<QuizAttemptDto>>.Failure("Quiz not found.", ServiceError.NotFound);

            var isQuizOwner = quiz.Subject.TeacherId == userId;

            var query = _context.QuizAttempts
                .Where(a => a.QuizId == quizId);

            if (!isQuizOwner)
            {
                query = query.Where(a => a.Enrollment.StudentId == userId);
            }

            query = query.OrderByDescending(a => a.StartTime);

            int totalCount = await query.CountAsync();

            var attempts = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new QuizAttemptDto(
                    a.Id,
                    a.QuizId,
                    a.EnrollmentId,
                    a.StartTime,
                    a.EndTime,
                    a.Status,
                    a.TotalScore,
                    a.Mark
                ))
                .ToListAsync();

            return ServiceResult<PagedResponse<QuizAttemptDto>>.Ok(new PagedResponse<QuizAttemptDto>
            {
                Data = attempts,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        public async Task<ServiceResult<QuestionResponseDto>> GradeFreeTextResponseAsync(long responseId, long teacherId, GradeQuestionResponseDto dto)
        {
            var response = await _context.QuizQuestionResponses
                .Include(r => r.Options)
                .Include(r => r.QuizAttempt)
                    .ThenInclude(a => a.Quiz)
                        .ThenInclude(q => q.Subject)
                .Include(r => r.QuizAttempt)
                    .ThenInclude(a => a.Responses) // Need all responses to recalculate total score
                .FirstOrDefaultAsync(r => r.Id == responseId);

            if (response == null) return ServiceResult<QuestionResponseDto>.Failure("Response not found.", ServiceError.NotFound);

            // Security Check
            if (response.QuizAttempt.Quiz.Subject.TeacherId != teacherId)
                return ServiceResult<QuestionResponseDto>.Failure("You can only grade responses for your own subjects.", ServiceError.Forbidden);

            // 1. Update the manual grade and like status
            response.Score = dto.Score;
            response.IsLiked = dto.IsLiked;

            // 2. Recalculate the overall QuizAttempt TotalScore since the teacher changed a grade!
            response.QuizAttempt.TotalScore = response.QuizAttempt.Responses.Sum(r => r.Score);

            await _context.SaveChangesAsync();

            return ServiceResult<QuestionResponseDto>.Ok(new QuestionResponseDto(
                response.Id, response.QuizQuestionId, response.ResponseTimeMs,
                response.FreeTextAnswer, response.IsLiked, response.Score,
                response.Options.Select(o => o.Id).ToList()));
        }

        public async Task<ServiceResult<bool>> DeleteAttemptAsync(long attemptId, long teacherId)
        {
            var attempt = await _context.QuizAttempts
                .Include(a => a.Quiz)
                    .ThenInclude(q => q.Subject)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null)
                return ServiceResult<bool>.Failure("Attempt not found.", ServiceError.NotFound);

            if (attempt.Quiz.Subject.TeacherId != teacherId)
                return ServiceResult<bool>.Failure("You do not have permission to delete attempts for this subject.", ServiceError.Forbidden);

            _context.QuizAttempts.Remove(attempt);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }
    }
}