namespace Sqeez.Api.DTOs
{
    public class SchoolClassDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public long? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public int StudentCount { get; set; }
    }

    public class SchoolClassFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SearchTerm { get; set; } // Search against Name or Section
        public string? AcademicYear { get; set; }
        public long? TeacherId { get; set; }
    }

    public class CreateSchoolClassDto
    {
        public string Name { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public long? TeacherId { get; set; }
    }

    public class UpdateSchoolClassDto
    {
        public string Name { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public long? TeacherId { get; set; }
    }
}