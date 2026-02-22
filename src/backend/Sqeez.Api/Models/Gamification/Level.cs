namespace Sqeez.Api.Models.Gamification
{
    public class Level
    {
        public int Id { get; set; } // Maps to _levelId
        public int XpThreshold { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}