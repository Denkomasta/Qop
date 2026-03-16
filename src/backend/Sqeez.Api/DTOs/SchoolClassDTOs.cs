namespace Sqeez.Api.DTOs
{
    public record SchoolClassDto(
        long Id,
        string Name,
        string AcademicYear,
        string Section,
        long? TeacherId,
        string? TeacherName,
        int StudentCount,
        int SubjectCount);

    public class SchoolClassFilterDto : PagedFilterDto
    {
        public string? SearchTerm { get; set; }  // Search against Name or Section
        public string? AcademicYear { get; set; }
        public long? TeacherId { get; set; }
    }

    public record CreateSchoolClassDto(
        string Name,
        string AcademicYear,
        string Section,
        long? TeacherId);

    public record PatchSchoolClassDto(
        string? Name = null,
        string? AcademicYear = null,
        string? Section = null,
        long? TeacherId = null);

    public record AssignStudentsDto(List<long> StudentIds);
    public record RemoveStudentsDto(List<long> StudentIds);

    public record SchoolClassDetailDto
    {
        public long Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string AcademicYear { get; init; } = string.Empty;
        public string Section { get; init; } = string.Empty;

        public TeacherBasicDto? Teacher { get; init; }
        public List<ClassmateDto> Students { get; init; } = new();
        public List<SubjectBasicDto> Subjects { get; init; } = new();
    }
}