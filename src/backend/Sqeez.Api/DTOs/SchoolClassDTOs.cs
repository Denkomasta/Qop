namespace Sqeez.Api.DTOs
{
    public record SchoolClassDto(
        long Id,
        string Name,
        string AcademicYear,
        string Section,
        long? TeacherId,
        string? TeacherName,
        int StudentCount);

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
        string? Name,
        string? AcademicYear,
        string? Section,
        long? TeacherId);

    public record AssignStudentsDto(List<long> StudentIds);
    public record RemoveStudentsDto(List<long> StudentIds);
}