namespace Sqeez.Api.Models.Gamification
{
    public class Badge
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public int XpBonus { get; set; }
        public string Condition { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<StudentBadge> StudentBadges { get; set; } = new List<StudentBadge>();
    }
}