using Sqeez.Api.Enums;

namespace Sqeez.Api.DTOs
{
    public class StudentCreateDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class StudentResponseDTO
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int CurrentXP { get; set; }
        public UserRole Role { get; set; }
        public DateTime LastSeen { get; set; }
    }
}