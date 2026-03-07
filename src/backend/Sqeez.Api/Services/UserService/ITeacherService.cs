using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.UserService
{
    public interface ITeacherService
    {
        Task<ServiceResult<PagedResponse<TeacherDto>>> GetAllTeachersAsync(int pageNumber = 1, int pageSize = 10);
        Task<ServiceResult<TeacherDto>> GetTeacherByIdAsync(long id);
        Task<ServiceResult<TeacherDto>> CreateTeacherAsync(CreateTeacherDto dto);
        Task<ServiceResult<bool>> UpdateTeacherAsync(long id, UpdateTeacherDto dto);
        Task<ServiceResult<bool>> DeleteTeacherAsync(long id);
    }
}