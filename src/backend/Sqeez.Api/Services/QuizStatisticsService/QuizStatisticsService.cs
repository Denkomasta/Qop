using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Services
{
    public class QuizStatisticsService : BaseService<QuizStatisticsService>, IQuizStatisticsService
    {
        public QuizStatisticsService(SqeezDbContext context, ILogger<QuizStatisticsService> logger)
            : base(context, logger)
        {
        }

        public async Task<ServiceResult<QuizSummaryStatDto>> GetQuizSummaryStatsAsync(long quizId, long teacherId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Subject)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                return ServiceResult<QuizSummaryStatDto>.Failure("Quiz not found.", ServiceError.NotFound);

            if (quiz.Subject.TeacherId != teacherId)
                return ServiceResult<QuizSummaryStatDto>.Failure("You do not have permission to view statistics for this quiz.", ServiceError.Forbidden);

            var attemptsQuery = _context.QuizAttempts
                .AsNoTracking()
                .Where(a => a.QuizId == quizId);

            var totalAttempts = await attemptsQuery.CountAsync();

            if (totalAttempts == 0)
            {
                return ServiceResult<QuizSummaryStatDto>.Ok(new QuizSummaryStatDto { QuizId = quizId });
            }

            var completedAttempts = await attemptsQuery
                .Where(a => a.Status == AttemptStatus.Completed)
                .ToListAsync();

            var summary = new QuizSummaryStatDto
            {
                QuizId = quizId,
                TotalAttempts = totalAttempts,
                CompletedAttempts = completedAttempts.Count,
                AverageScore = completedAttempts.Any() ? completedAttempts.Average(a => a.TotalScore) : 0,
                HighestScore = completedAttempts.Any() ? completedAttempts.Max(a => a.TotalScore) : 0,
                LowestScore = completedAttempts.Any() ? completedAttempts.Min(a => a.TotalScore) : 0,
                AverageCompletionTimeMinutes = completedAttempts
                    .Where(a => a.StartTime.HasValue && a.EndTime.HasValue)
                    .Select(a => (a.EndTime!.Value - a.StartTime!.Value).TotalMinutes)
                    .DefaultIfEmpty(0)
                    .Average()
            };

            return ServiceResult<QuizSummaryStatDto>.Ok(summary);
        }

        public async Task<ServiceResult<IEnumerable<QuestionStatDto>>> GetQuestionStatsAsync(long quizId, long teacherId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Subject)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                return ServiceResult<IEnumerable<QuestionStatDto>>.Failure("Quiz not found.", ServiceError.NotFound);

            if (quiz.Subject.TeacherId != teacherId)
                return ServiceResult<IEnumerable<QuestionStatDto>>.Failure("You do not have permission to view statistics for this quiz.", ServiceError.Forbidden);

            var questionStats = await _context.QuizQuestions
                .AsNoTracking()
                .Where(q => q.QuizId == quizId)
                .Select(q => new QuestionStatDto
                {
                    Id = q.Id,
                    QuestionText = q.Title,

                    TotalAnswers = _context.QuizQuestionResponses
                        .Count(r => r.QuizQuestionId == q.Id && r.QuizAttempt.Status == AttemptStatus.Completed),

                    Options = q.Options.Select(o => new OptionStatDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect,
                        PickCount = o.Responses.Count(r => r.QuizAttempt.Status == AttemptStatus.Completed)
                    }).ToList(),

                    AverageScore = _context.QuizQuestionResponses
                        .Where(r => r.QuizQuestionId == q.Id && r.QuizAttempt.Status == AttemptStatus.Completed)
                        .Average(r => r.Score ?? 0),

                    AverageResponseTimeSeconds = _context.QuizQuestionResponses
                        .Where(r => r.QuizQuestionId == q.Id && r.QuizAttempt.Status == AttemptStatus.Completed)
                        .Average(r => r.ResponseTimeMs) / 1000.0
                })
                .ToListAsync();

            return ServiceResult<IEnumerable<QuestionStatDto>>.Ok(questionStats);
        }
    }
}