using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult<PagedResponse<StudentDto>>> GetAllUsersAsync(UserFilterDto filter);
        Task<ServiceResult<StudentDto>> GetUserByIdAsync(long id);
        Task<ServiceResult<StudentDto>> CreateUserAsync(CreateStudentDto dto);
        Task<ServiceResult<StudentDto>> PatchUserAsync(long id, PatchStudentDto dto);
        Task<ServiceResult<bool>> ArchiveUserAsync(long id);
        Task<ServiceResult<string>> UploadAvatarAsync(long userId, IFormFile imageFile);
    }
}