namespace Sqeez.Api.DTOs
{
    public record SubjectDto(
        long Id,
        string Name,
        string Code,
        string? Description,
        DateTime StartDate,
        DateTime? EndDate,
        long? TeacherId,
        string? TeacherName,
        long? SchoolClassId,
        string? SchoolClassName,
        int EnrollmentCount,    // Enrollements are added in Enrollment service
        int QuizCount           // Quizzes are added in Quiz service
        );

    public class SubjectFilterDto : PagedFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Code { get; set; }

        public long? TeacherId { get; set; }
        public long? SchoolClassId { get; set; }

        public bool? IsActive { get; set; }
        public DateTime? StartingAfter { get; set; }

        public bool IsDescending { get; set; } = false;
    }

    public record CreateSubjectDto(
        string Name,
        string Code,
        string? Description = null,
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        long? TeacherId = null,
        long? SchoolClassId = null);

    public record PatchSubjectDto(
        string? Name = null,
        string? Code = null,
        string? Description = null,
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        long? TeacherId = null,     // Pass 0 to remove the teacher
        long? SchoolClassId = null  // Pass 0 to remove the class
    );
    public record SubjectBasicDto
    {
        public long Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
    }
}