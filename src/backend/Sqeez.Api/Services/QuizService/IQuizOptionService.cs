using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface IQuizOptionService
    {
        Task<ServiceResult<PagedResponse<QuizOptionDto>>> GetAllQuizOptionsAsync(QuizOptionFilterDto filter, long currentUserId, bool isAdmin);
        Task<ServiceResult<QuizOptionDto>> GetQuizOptionByIdAsync(long id, long currentUserId);

        Task<ServiceResult<QuizOptionDto>> CreateQuizOptionAsync(CreateQuizOptionDto dto, long currentUserId);
        Task<ServiceResult<QuizOptionDto>> PatchQuizOptionAsync(long id, PatchQuizOptionDto dto, long currentUserId);
        Task<ServiceResult<bool>> DeleteQuizOptionAsync(long id, long currentUserId, bool isAdmin);
    }
}
