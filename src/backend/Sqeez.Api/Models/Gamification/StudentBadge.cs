using Sqeez.Api.Models.Users;

namespace Sqeez.Api.Models.Gamification
{
    public class StudentBadge
    {
        public DateTime EarnedAt { get; set; }

        // Composite Foreign Keys
        public long StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public long BadgeId { get; set; }
        public Badge Badge { get; set; } = null!;
    }
}