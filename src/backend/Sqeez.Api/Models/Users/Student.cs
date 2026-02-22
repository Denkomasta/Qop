using Sqeez.Api.Enums;
using Sqeez.Api.Models.Academics;
using Sqeez.Api.Models.Gamification;

namespace Sqeez.Api.Models.Users
{
    public class Student
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int CurrentXP { get; set; }
        public UserRole Role { get; set; }
        public DateTime LastSeen { get; set; }
        public bool IsArchived { get; set; }
        public bool IsOnline { get; set; }

        // Navigation Properties
        public long? SchoolClassId { get; set; }
        public SchoolClass? SchoolClass { get; set; }

        public ICollection<StudentBadge> StudentBadges { get; set; } = new List<StudentBadge>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}