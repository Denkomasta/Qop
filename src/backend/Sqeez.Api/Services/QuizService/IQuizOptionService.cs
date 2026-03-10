using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface IQuizOptionService
    {
        Task<ServiceResult<PagedResponse<QuizOptionDto>>> GetAllQuizOptionsAsync(QuizOptionFilterDto filter);

        Task<ServiceResult<QuizOptionDto>> GetQuizOptionByIdAsync(long id);
        Task<ServiceResult<QuizOptionDto>> CreateQuizOptionAsync(CreateQuizOptionDto dto);
        Task<ServiceResult<QuizOptionDto>> PatchQuizOptionAsync(long id, PatchQuizOptionDto dto);
        Task<ServiceResult<bool>> DeleteQuizOptionAsync(long id);
    }
}
