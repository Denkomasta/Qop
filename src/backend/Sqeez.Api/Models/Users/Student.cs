using Sqeez.Api.Enums;
using Sqeez.Api.Models.Academics;
using Sqeez.Api.Models.Gamification;

namespace Sqeez.Api.Models.Users
{
    public class Student
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int CurrentXP { get; set; }
        public UserRole Role { get; set; }
        public DateTime LastSeen { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }
        public string? PendingNewEmail { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        public long? SchoolClassId { get; set; }
        public SchoolClass? SchoolClass { get; set; }

        public ICollection<StudentBadge> StudentBadges { get; set; } = new List<StudentBadge>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}