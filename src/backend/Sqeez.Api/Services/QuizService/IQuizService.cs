using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface IQuizService
    {
        Task<ServiceResult<PagedResponse<QuizDto>>> GetAllQuizzesAsync(QuizFilterDto filter);
        Task<ServiceResult<QuizDto>> GetQuizByIdAsync(long id, GetQuizDto dto);
        Task<ServiceResult<QuizDto>> CreateQuizAsync(CreateQuizDto dto, long currentUserId);
        Task<ServiceResult<QuizDto>> PatchQuizAsync(long id, PatchQuizDto dto, long currentUserId);
        Task<ServiceResult<bool>> DeleteQuizAsync(long id, long currentUserId, bool isAdmin);
    }
}