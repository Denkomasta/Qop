using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.UserService
{
    public interface IStudentService
    {
        Task<ServiceResult<PagedResponse<StudentDto>>> GetAllStudentsAsync(StudentFilterDto filter);
        Task<ServiceResult<StudentDto>> GetStudentByIdAsync(long id);
        Task<ServiceResult<StudentDto>> CreateStudentAsync(CreateStudentDto dto);
        Task<ServiceResult<bool>> PatchStudentAsync(long id, PatchStudentDto dto);
        Task<ServiceResult<bool>> DeleteStudentAsync(long id);
    }
}