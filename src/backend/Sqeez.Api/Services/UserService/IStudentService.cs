using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.UserService
{
    public interface IStudentService
    {
        Task<ServiceResult<PagedResponse<StudentDto>>> GetAllStudentsAsync(int pageNumber = 1, int pageSize = 10);
        Task<ServiceResult<StudentDto>> GetStudentByIdAsync(long id);
        Task<ServiceResult<StudentDto>> CreateStudentAsync(CreateStudentDto dto);
        Task<ServiceResult<bool>> PatchStudentAsync(long id, PatchStudentDto dto);
        Task<ServiceResult<bool>> DeleteStudentAsync(long id);
    }
}