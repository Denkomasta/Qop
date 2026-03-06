using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.AuthService
{
    public interface IAuthService
    {
        Task<ServiceResult<string>> RegisterAsync(RegisterDTO dto);
        Task<ServiceResult<string>> LoginAsync(LoginDTO loginDto);
        Task<ServiceResult<bool>> LogoutAsync(long userId);
        Task<ServiceResult<UserDTO>> GetCurrentUserAsync(long userId, string? role);
        Task<ServiceResult<bool>> UpdateUserRoleAsync(long adminId, UpdateRoleDTO dto);
    }
}