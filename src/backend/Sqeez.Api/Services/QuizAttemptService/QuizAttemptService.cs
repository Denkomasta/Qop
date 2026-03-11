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
        public QuizAttemptService(SqeezDbContext context, ILogger<QuizAttemptService> logger)
            : base(context, logger) { }

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

            return ServiceResult<QuizAttemptDto>.Ok(new QuizAttemptDto(
                attempt.Id, attempt.QuizId, attempt.EnrollmentId, attempt.StartTime,
                attempt.EndTime, attempt.Status, attempt.TotalScore, attempt.Mark));
        }

        public async Task<ServiceResult<QuestionResponseDto>> SubmitAnswerAsync(long attemptId, long studentId, SubmitQuestionResponseDto dto)
        {
            // Verify the Attempt is valid, belongs to the student, and is still InProgress
            var attempt = await _context.QuizAttempts
                .Include(a => a.Enrollment)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return ServiceResult<QuestionResponseDto>.Failure("Attempt not found.", ServiceError.NotFound);
            if (attempt.Enrollment.StudentId != studentId) return ServiceResult<QuestionResponseDto>.Failure("Access denied.", ServiceError.Forbidden);

            if (attempt.Status == AttemptStatus.Created) attempt.Status = AttemptStatus.Started;
            if (attempt.Status != AttemptStatus.Started) return ServiceResult<QuestionResponseDto>.Failure("This quiz attempt is no longer in progress.", ServiceError.Conflict);

            // Fetch the specific question and its correct options for Auto-Grading
            var question = await _context.QuizQuestions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == dto.QuizQuestionId && q.QuizId == attempt.QuizId);

            if (question == null) return ServiceResult<QuestionResponseDto>.Failure("Question not found on this quiz.", ServiceError.NotFound);

            // Fetch the existing response (if the student is changing their answer) or create a new one
            var response = await _context.QuizQuestionResponses
                .Include(r => r.Options)
                .FirstOrDefaultAsync(r => r.QuizAttemptId == attemptId && r.QuizQuestionId == dto.QuizQuestionId);

            if (response == null)
            {
                response = new QuizQuestionResponse
                {
                    QuizAttemptId = attemptId,
                    QuizQuestionId = dto.QuizQuestionId,
                };
                _context.QuizQuestionResponses.Add(response);
            }

            // Update basic fields
            response.ResponseTimeMs = dto.ResponseTimeMs;
            response.FreeTextAnswer = dto.FreeTextAnswer;

            // Handle the Many-to-Many Options relationship securely
            response.Options.Clear(); // Remove old answers from the join table

            if (dto.SelectedOptionIds != null && dto.SelectedOptionIds.Any())
            {
                // Only allow them to select options that actually belong to THIS question!
                var validOptions = question.Options.Where(o => dto.SelectedOptionIds.Contains(o.Id)).ToList();
                foreach (var opt in validOptions)
                {
                    response.Options.Add(opt);
                }
            }

            // Auto-Grading Engine
            response.Score = 0;

            // If it's a Free Text answer
            if (!string.IsNullOrWhiteSpace(dto.FreeTextAnswer))
            {
                var correctTextOption = question.Options.FirstOrDefault(o => o.IsFreeText && o.IsCorrect);
                if (correctTextOption?.Text != null && correctTextOption.Text.Equals(dto.FreeTextAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    response.Score = question.Difficulty;
                }
            }
            // If it's a Multiple Choice answer
            else if (response.Options.Any())
            {
                var correctOptionIds = question.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList();
                var selectedIds = response.Options.Select(o => o.Id).ToList();

                // They only get points if they selected EXACTLY the correct options (no extra wrong ones, no missing right ones)
                bool isPerfectMatch = correctOptionIds.Count == selectedIds.Count && !correctOptionIds.Except(selectedIds).Any();

                if (isPerfectMatch)
                {
                    response.Score = question.Difficulty;
                }
            }

            await _context.SaveChangesAsync();

            return ServiceResult<QuestionResponseDto>.Ok(new QuestionResponseDto(
                response.Id, response.QuizQuestionId, response.ResponseTimeMs,
                response.FreeTextAnswer, response.IsLiked, response.Score,
                response.Options.Select(o => o.Id).ToList()));
        }

        public async Task<ServiceResult<QuizAttemptDto>> CompleteAttemptAsync(long attemptId, long studentId)
        {
            // Fetch the attempt and responses so we can calculate the final score
            var attempt = await _context.QuizAttempts
                .Include(a => a.Enrollment)
                .Include(a => a.Responses)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return ServiceResult<QuizAttemptDto>.Failure("Attempt not found.", ServiceError.NotFound);
            if (attempt.Enrollment.StudentId != studentId) return ServiceResult<QuizAttemptDto>.Failure("Access denied.", ServiceError.Forbidden);
            if (attempt.Status != AttemptStatus.Started) return ServiceResult<QuizAttemptDto>.Failure("This attempt is already completed.", ServiceError.Conflict);

            // Lock it down
            attempt.Status = AttemptStatus.Completed;
            attempt.EndTime = DateTime.UtcNow;

            // Calculate Final Score based on the auto-graded answers
            attempt.TotalScore = attempt.Responses.Sum(r => r.Score);

            await _context.SaveChangesAsync();

            return ServiceResult<QuizAttemptDto>.Ok(new QuizAttemptDto(
                attempt.Id, attempt.QuizId, attempt.EnrollmentId, attempt.StartTime,
                attempt.EndTime, attempt.Status, attempt.TotalScore, attempt.Mark));
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

        public async Task<ServiceResult<PagedResponse<QuizAttemptDto>>> GetAttemptsForQuizAsync(long quizId, long teacherId, int pageNumber = 1, int pageSize = 20)
        {
            // Verify the teacher actually owns this quiz via the Subject
            var quiz = await _context.Quizzes
                .Include(q => q.Subject)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return ServiceResult<PagedResponse<QuizAttemptDto>>.Failure("Quiz not found.", ServiceError.NotFound);
            if (quiz.Subject.TeacherId != teacherId) return ServiceResult<PagedResponse<QuizAttemptDto>>.Failure("You do not have permission to view attempts for this quiz.", ServiceError.Forbidden);

            var query = _context.QuizAttempts
                .Where(a => a.QuizId == quizId)
                .OrderByDescending(a => a.StartTime);

            int totalCount = await query.CountAsync();

            var attempts = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new QuizAttemptDto(
                    a.Id, a.QuizId, a.EnrollmentId, a.StartTime,
                    a.EndTime, a.Status, a.TotalScore, a.Mark))
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
    }
}