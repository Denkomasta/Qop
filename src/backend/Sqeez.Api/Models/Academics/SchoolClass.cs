using Sqeez.Api.Models.Users;

namespace Sqeez.Api.Models.Academics
{
    public class SchoolClass
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;

        // Foreign Key for Teacher
        public long? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        // Navigation Properties
        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}