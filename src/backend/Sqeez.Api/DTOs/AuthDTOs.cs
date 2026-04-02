using Sqeez.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sqeez.Api.DTOs
{
    public record LoginDTO(
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        string Email,

        string Password,

        bool RememberMe = false
    );

    public record UserDTO(
        long Id,
        string Username,
        string Email,
        string CurrentXP,
        UserRole Role,
        string? AvatarUrl
    );

    public record RegisterDTO(
     [RegularExpression(@"^[a-zA-Z \-áéíóúýčďěňřšťžÁÉÍÓÚÝČĎĚŇŘŠŤŽ]+$", ErrorMessage = "First name can only contain letters, spaces, and dashes.")]
    string FirstName,

     [RegularExpression(@"^[a-zA-Z \-áéíóúýčďěňřšťžÁÉÍÓÚÝČĎĚŇŘŠŤŽ]+$", ErrorMessage = "Last name can only contain letters, spaces, and dashes.")]
    string LastName,

     [RegularExpression(@"^[a-zA-Z0-9_\-áéíóúýčďěňřšťžÁÉÍÓÚÝČĎĚŇŘŠŤŽ]+$", ErrorMessage = "Username can only contain letters, numbers, dashes, and underscores.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters.")]
    string Username,

     [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
    string Email,

     // It enforces: 1 lower, 1 upper, 1 digit, 1 special char, and min 8 characters.
     [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$", ErrorMessage = "Password must be at least 8 characters long and contain an uppercase letter, a lowercase letter, a number, and a special character.")]
     string Password,

        bool RememberMe = false
     );

    public record UpdateRoleDTO(
    [Required]
    long Id,

    [Required]
    UserRole Role,

    [RegularExpression(@"^[a-zA-Z0-9_ \-áéíóúýčďěňřšťžÁÉÍÓÚÝČĎĚŇŘŠŤŽ.,&]+$", ErrorMessage = "Department contains invalid characters. No HTML tags allowed.")]
    string? Department = null,

    [RegularExpression(@"^\+?[0-9\s\-()]{7,15}$", ErrorMessage = "Phone number must be between 7 and 15 characters and contain only valid phone symbols.")]
    string? PhoneNumber = null
    );

    public record AuthResponseDto(
        string AccessToken,
        string RefreshToken
    );

    public record RefreshTokenDto(
        string RefreshToken
    );

    public record ResendVerificationDto(
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        string Email,
        bool RememberMe = false
    );

    public record ForgotPasswordDto(
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        string Email
    );

    public record ResetPasswordDto(
        string Token,

        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$", ErrorMessage = "Password must be at least 8 characters long and contain an uppercase letter, a lowercase letter, a number, and a special character.")]
        string NewPassword
    );
}