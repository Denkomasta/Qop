using Sqeez.Api.Enums;

namespace Sqeez.Api.DTOs
{
    public record LoginDTO(string Email, string Password, bool RememberMe = false);

    public record UserDTO(
        long Id,
        string Username,
        string Email,
        string CurrentXP,
        UserRole Role,
        string? AvatarUrl
    );

    public record RegisterDTO(string FirstName, string LastName, string Username, string Email, string Password, bool RememberMe = false);
    public record UpdateRoleDTO(long Id, UserRole Role, string? Department = null, string? PhoneNumber = null);

    public record AuthResponseDto(
        string AccessToken,
        string RefreshToken
    );

    public record RefreshTokenDto(
        string RefreshToken
    );
}