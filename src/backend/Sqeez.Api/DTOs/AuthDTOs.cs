using Sqeez.Api.Enums;

namespace Sqeez.Api.DTOs
{
    public record LoginDTO(string Email, string Password, bool RememberMe = false);

    public record UserDTO(  // TODO add classes and enrollments
        long Id,
        string Username,
        string Email,
        string CurrentXP,
        string Role,
        string? Department, // For teachers
        string? PhoneNumber // For admins
    );

    public record RegisterDTO(string Username, string Email, string Password, bool RememberMe = false);
    public record UpdateRoleDTO(long Id, UserRole Role, string? Department = null, string? PhoneNumber = null);

    public record AuthResponseDto(
        string AccessToken,
        string RefreshToken
    );

    public record RefreshTokenDto(
        string RefreshToken
    );
}