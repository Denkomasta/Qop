using Sqeez.Api.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Sqeez.Api.DTOs
{
    [JsonDerivedType(typeof(StudentDto), typeDiscriminator: "student")]
    [JsonDerivedType(typeof(TeacherDto), typeDiscriminator: "teacher")]
    [JsonDerivedType(typeof(AdminDto), typeDiscriminator: "admin")]
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

    public record TeacherDto : StudentDto
    {
        public string? Department { get; init; }
        public long? ManagedClassId { get; init; }
    }

    public record AdminDto : TeacherDto
    {
        public string PhoneNumber { get; init; } = string.Empty;
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
    [JsonDerivedType(typeof(CreateStudentDto), typeDiscriminator: "student")]
    [JsonDerivedType(typeof(CreateTeacherDto), typeDiscriminator: "teacher")]
    [JsonDerivedType(typeof(CreateAdminDto), typeDiscriminator: "admin")]
    public record CreateStudentDto
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;

        [RegularExpression(@"^[a-zA-Z0-9_\-áéíóúýčďěňřšťžÁÉÍÓÚÝČĎĚŇŘŠŤŽ]+$")]
        public string Username { get; init; } = string.Empty;

        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        public string Email { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;
        public long? SchoolClassId { get; init; }
    }

    public record CreateTeacherDto : CreateStudentDto
    {
        [RegularExpression(@"^[a-zA-Z0-9_ \-áéíóúýčďěňřšťžÁÉÍÓÚÝČĎĚŇŘŠŤŽ.,&]+$")]
        public string? Department { get; init; }
        public long? ManagedClassId { get; init; }
    }

    public record CreateAdminDto : CreateTeacherDto
    {
        [RegularExpression(@"^00[1-9][0-9]{0,2}[0-9]{7,12}$",
            ErrorMessage = "Phone number must start with 00, followed by a country code and your number.")]
        public string? PhoneNumber { get; init; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
    [JsonDerivedType(typeof(PatchStudentDto), typeDiscriminator: "student")]
    [JsonDerivedType(typeof(PatchTeacherDto), typeDiscriminator: "teacher")]
    [JsonDerivedType(typeof(PatchAdminDto), typeDiscriminator: "admin")]
    public record PatchStudentDto
    {
        [RegularExpression(@"^[a-zA-Z0-9_\-áéíóúýčďěňřšťžÁÉÍÓÚÝČĎĚŇŘŠŤŽ]+$")]
        public string? Username { get; init; }

        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        public string? Email { get; init; }

        public long? SchoolClassId { get; init; }
        public string? AvatarUrl { get; init; }
    }

    public record PatchTeacherDto : PatchStudentDto
    {
        [RegularExpression(@"^[a-zA-Z0-9_ \-áéíóúýčďěňřšťžÁÉÍÓÚÝČĎĚŇŘŠŤŽ.,&]+$")]
        public string? Department { get; init; }
        public long? ManagedClassId { get; init; }
    }

    public record PatchAdminDto : PatchTeacherDto
    {
        [RegularExpression(@"^00[1-9][0-9]{0,2}[0-9]{7,12}$",
            ErrorMessage = "Phone number must start with 00, followed by a country code and your number.")]
        public string? PhoneNumber { get; init; }
    }

    public enum UserSortField
    {
        Username,
        XP,
        LastSeen
    }

    public class UserFilterDto : PagedFilterDto
    {
        public string? SearchTerm { get; init; }
        public bool? IsOnline { get; init; }
        public long? SchoolClassId { get; init; }
        public bool? IsArchived { get; init; }

        public UserRole? Role { get; init; }
        public bool StrictRoleOnly { get; init; } = false;

        public string? Department { get; init; }
        public string? PhoneNumber { get; init; }
        public UserSortField? SortBy { get; init; }
        public bool IsDescending { get; init; } = false;
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
        public string? AvatarUrl { get; init; }
    }

    public record DetailedUserDto : AdminDto
    {
        public SchoolClassBasicDto? SchoolClassDetails { get; init; }
        public List<StudentBadgeBasicDto> Badges { get; init; } = new();
        public List<EnrollmentBasicDto> Enrollments { get; init; } = new();
    }
}