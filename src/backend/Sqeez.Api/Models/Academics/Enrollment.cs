using Sqeez.Api.Models.Users;
using Sqeez.Api.Models.QuizSystem;

namespace Sqeez.Api.Models.Academics
{
    public class Enrollment
    {
        public long Id { get; set; }
        public int Mark { get; set; }
        public DateTime EnrolledAt { get; set; }
        public bool IsActive { get; set; }

        // Foreign Keys
        public long StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public long SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        // Navigation Properties
        public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    }
}