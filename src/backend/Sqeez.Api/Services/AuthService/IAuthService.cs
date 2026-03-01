using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.AuthService
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(StudentCreateDTO dto);
        Task<string?> LoginAsync(LoginDTO loginDto);
        Task<bool> LogoutAsync(long userId);
    }
}