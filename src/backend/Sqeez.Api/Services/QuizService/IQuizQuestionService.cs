using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface IQuizQuestionService
    {
        Task<ServiceResult<PagedResponse<QuizQuestionDto>>> GetAllQuizQuestionsAsync(QuizQuestionFilterDto filter);
        Task<ServiceResult<QuizQuestionDto>> GetQuizQuestionByIdAsync(long id);
        Task<ServiceResult<DetailedQuizQuestionDto>> GetDetailedQuizQuestionByIdAsync(long id, long quizId);
        Task<ServiceResult<QuizQuestionDto>> CreateQuizQuestionAsync(CreateQuizQuestionDto dto, long currentUserId);
        Task<ServiceResult<QuizQuestionDto>> PatchQuizQuestionAsync(long id, PatchQuizQuestionDto dto, long currentUserId);
        Task<ServiceResult<bool>> DeleteQuizQuestionAsync(long id, long currentUserId, bool isAdmin);
    }
}
