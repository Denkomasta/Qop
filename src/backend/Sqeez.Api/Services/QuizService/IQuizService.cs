using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface IQuizService
    {
        Task<ServiceResult<PagedResponse<QuizDto>>> GetAllQuizzesAsync(QuizFilterDto filter);

        Task<ServiceResult<QuizDto>> GetQuizByIdAsync(long id);
        Task<ServiceResult<QuizDto>> CreateQuizAsync(CreateQuizDto dto);
        Task<ServiceResult<QuizDto>> PatchQuizAsync(long id, PatchQuizDto dto);
        Task<ServiceResult<bool>> DeleteQuizAsync(long id);
    }
}