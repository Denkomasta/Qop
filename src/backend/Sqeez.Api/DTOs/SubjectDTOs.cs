namespace Sqeez.Api.DTOs
{
    public record SubjectDto(
        long Id,
        string Name,
        string Code,
        string? Description,
        DateTime StartDate,
        DateTime? EndDate,
        bool IsActive,
        long? TeacherId,
        string? TeacherName,
        long? SchoolClassId,
        string? SchoolClassName,
        int EnrollmentCount,    // Enrollements are added in Enrollment service
        int QuizCount           // Quizzes are added in Quiz service
        );

    public record SubjectFilterDto : PagedFilterDto
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
        string? Description,
        DateTime? StartDate,
        DateTime? EndDate,
        long? TeacherId,
        long? SchoolClassId);

    public record PatchSubjectDto(
        string? Name,
        string? Code,
        string? Description,
        DateTime? StartDate,
        DateTime? EndDate,
        bool? IsActive,
        long? TeacherId,     // Pass 0 to remove the teacher
        long? SchoolClassId  // Pass 0 to remove the class
    );
}