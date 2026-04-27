using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface IQuizStatisticsService
    {
        Task<ServiceResult<QuizSummaryStatDto>> GetQuizSummaryStatsAsync(long quizId, long teacherId);
        Task<ServiceResult<IEnumerable<QuestionStatDto>>> GetQuestionStatsAsync(long quizId, long teacherId);
    }
}