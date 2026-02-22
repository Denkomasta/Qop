using Microsoft.VisualBasic.FileIO;
using Sqeez.Api.Models.Media;

namespace Sqeez.Api.Models.QuizSystem
{
    public class QuizQuestion
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public int Difficulty { get; set; }
        public int TimeLimit { get; set; }

        // Foreign Keys
        public long QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        public long? MediaAssetId { get; set; }
        public MediaAsset? Media { get; set; }

        // Navigation Properties
        public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
    }
}