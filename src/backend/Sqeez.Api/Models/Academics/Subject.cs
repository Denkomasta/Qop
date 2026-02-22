using Sqeez.Api.Models.Users;
using Sqeez.Api.Models.QuizSystem;

namespace Sqeez.Api.Models.Academics
{
    public class Subject
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }

        // Foreign Keys
        public long? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        public long? SchoolClassId { get; set; }
        public SchoolClass? SchoolClass { get; set; }

        // Navigation Properties
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}