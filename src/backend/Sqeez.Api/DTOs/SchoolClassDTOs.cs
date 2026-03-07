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
        int PageNumber = 1,
        int PageSize = 10,
        string? SearchTerm = null,  // Search against Name or Section
        string? AcademicYear = null,
        long? TeacherId = null);

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