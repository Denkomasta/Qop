using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.AuthService
{
    public interface IAuthService
    {
        Task<ServiceResult<bool>> RegisterAsync(RegisterDTO dto);
        Task<ServiceResult<AuthResponseDto>> VerifyEmailAsync(string token, bool rememberMe);
        Task<ServiceResult<bool>> ResendVerificationEmailAsync(ResendVerificationDto dto);
        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDTO dto);
        Task<ServiceResult<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
        Task<ServiceResult<bool>> LogoutAsync(long userId, string? refreshToken = null);
        Task<ServiceResult<UserDTO>> GetCurrentUserAsync(long userId, string? role);
        Task<ServiceResult<bool>> UpdateUserRoleAsync(long adminId, UpdateRoleDTO dto);
    }
}