namespace Sqeez.Api.Models.Gamification
{
    public class Badge
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public int XpBonus { get; set; }

        // Navigation Property
        public ICollection<BadgeRule> Rules { get; set; } = new List<BadgeRule>();
        public ICollection<StudentBadge> StudentBadges { get; set; } = new List<StudentBadge>();
    }
}