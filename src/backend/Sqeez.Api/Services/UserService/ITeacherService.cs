using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.UserService
{
    public interface ITeacherService
    {
        Task<ServiceResult<PagedResponse<TeacherDto>>> GetAllTeachersAsync(TeacherFilterDto filter);
        Task<ServiceResult<TeacherDto>> GetTeacherByIdAsync(long id);
        Task<ServiceResult<TeacherDto>> CreateTeacherAsync(CreateTeacherDto dto);
        Task<ServiceResult<TeacherDto>> PatchTeacherAsync(long id, PatchTeacherDto dto);
        Task<ServiceResult<bool>> DeleteTeacherAsync(long id);
    }
}