namespace Sqeez.Api.DTOs
{
    public record StudentDto
    {
        public long Id { get; init; }
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public int CurrentXP { get; init; }
        public string Role { get; init; } = string.Empty;
        public bool IsOnline { get; init; }
        public long? SchoolClassId { get; init; }
    }

    public record CreateStudentDto
    {
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public long? SchoolClassId { get; init; }
    }

    public record UpdateStudentDto
    {
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public long? SchoolClassId { get; init; }
    }

    public record TeacherDto : StudentDto
    {
        public string? Department { get; init; }
    }

    public record CreateTeacherDto : CreateStudentDto
    {
        public string? Department { get; init; }
    }

    public record UpdateTeacherDto : UpdateStudentDto
    {
        public string? Department { get; init; }
    }

    public record AdminDto : TeacherDto
    {
        public string PhoneNumber { get; init; } = string.Empty;
    }

    public record CreateAdminDto : CreateTeacherDto
    {
        public string? PhoneNumber { get; init; }
    }

    public record UpdateAdminDto : UpdateTeacherDto
    {
        public string? PhoneNumber { get; init; }
    }
}