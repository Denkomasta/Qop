using Sqeez.Api.Enums;
using Sqeez.Api.Models.Academics;

namespace Sqeez.Api.Models.QuizSystem
{
    public class QuizAttempt
    {
        public long Id { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public AttemptStatus Status { get; set; }
        public int TotalScore { get; set; }
        public int? Mark { get; set; }

        // Foreign Keys
        public long QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        public long EnrollmentId { get; set; }
        public Enrollment Enrollment { get; set; } = null!;

        // Navigation Property
        public ICollection<QuizQuestionResponse> Responses { get; set; } = new List<QuizQuestionResponse>();
    }
}