namespace Sqeez.Api.DTOs
{
    public record LoginDTO(string Email, string Password);

    public record AuthResponseDTO(
        long Id,
        string Username,
        string Token,
        string Role
    );

    public record RegisterDTO(string? Username, string Email, string Password);
}