using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.UserService
{
    public interface IAdminService
    {
        Task<ServiceResult<PagedResponse<AdminDto>>> GetAllAdminsAsync(AdminFilterDto filter);
        Task<ServiceResult<AdminDto>> GetAdminByIdAsync(long id);
        Task<ServiceResult<AdminDto>> CreateAdminAsync(CreateAdminDto dto);
        Task<ServiceResult<bool>> PatchAdminAsync(long id, PatchAdminDto dto);
        Task<ServiceResult<bool>> DeleteAdminAsync(long id);
    }
}