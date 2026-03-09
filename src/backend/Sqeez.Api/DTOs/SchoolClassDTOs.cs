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

    public record SchoolClassFilterDto(
        string? SearchTerm = null,  // Search against Name or Section
        string? AcademicYear = null,
        long? TeacherId = null) : PagedFilterDto;

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