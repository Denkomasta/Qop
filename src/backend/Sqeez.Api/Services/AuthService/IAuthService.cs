using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.AuthService
{
    public interface IAuthService
    {
        Task<ServiceResult<bool>> RegisterAsync(RegisterDTO dto);
        Task<ServiceResult<bool>> VerifyEmailAsync(string token);
        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDTO dto);
        Task<ServiceResult<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
        Task<ServiceResult<bool>> LogoutAsync(long userId, string? refreshToken = null);
        Task<ServiceResult<UserDTO>> GetCurrentUserAsync(long userId, string? role);
        Task<ServiceResult<bool>> UpdateUserRoleAsync(long adminId, UpdateRoleDTO dto);
    }
}