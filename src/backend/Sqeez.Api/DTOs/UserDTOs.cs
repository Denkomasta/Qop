using Sqeez.Api.Enums;
using System.Text.Json.Serialization;

namespace Sqeez.Api.DTOs
{
    public record StudentDto
    {
        public long Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public int CurrentXP { get; init; }
        public UserRole Role { get; init; }
        public string? AvatarUrl { get; init; }
        public DateTime LastSeen { get; init; }
        public long? SchoolClassId { get; init; }
    }

    public class StudentFilterDto : PagedFilterDto
    {   
        public string? SearchTerm { get; init; } // To search by Username or Email
        public bool? IsOnline { get; init; }
        public long? SchoolClassId { get; init; }
        
        public bool? IsArchived { get; init; }
        public bool StrictRoleOnly { get; init; } = false;
    }

    public record CreateStudentDto
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public long? SchoolClassId { get; init; }
    }

    public record PatchStudentDto
    {
        public string? Username { get; init; }
        public string? Email { get; init; }
        public long? SchoolClassId { get; init; }
        public string? AvatarUrl { get; init; }
    }

    public record TeacherDto : StudentDto
    {
        public string? Department { get; init; }
        public long? ManagedClassId { get; init; }
    }

    public class TeacherFilterDto : StudentFilterDto
    {
        public string? Department { get; init; }
    }

    public record CreateTeacherDto : CreateStudentDto
    {
        public string? Department { get; init; }
        public long? ManagedClassId { get; init; }
    }

    public record PatchTeacherDto : PatchStudentDto
    {
        public string? Department { get; init; }
        public long? ManagedClassId { get; init; }
    }

    public record AdminDto : TeacherDto
    {
        public string PhoneNumber { get; init; } = string.Empty;
    }

    public class AdminFilterDto : TeacherFilterDto
    {
        public string? PhoneNumber { get; init; }
        [JsonIgnore]
        public new bool StrictRoleOnly { get; init; } = false;
    }

    public record CreateAdminDto : CreateTeacherDto
    {
        public string? PhoneNumber { get; init; }
    }

    public record PatchAdminDto : PatchTeacherDto
    {
        public string? PhoneNumber { get; init; }
    }

    public record AvatarUploadResponseDto(string Message, string AvatarUrl);

    public record ClassmateDto
    {
        public long Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public int CurrentXp { get; init; }
        public string? AvatarUrl { get; init; }
    }

    public record TeacherBasicDto
    {
        public long Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }
}