using Sqeez.Api.Models.Media;

namespace Sqeez.Api.Models.QuizSystem
{
    public class QuizOption
    {
        public long Id { get; set; }
        public string? Text { get; set; }   // if isFreeText => answer of question, else text of option.
        public bool IsFreeText { get; set; }
        public bool IsCorrect { get; set; }

        // Foreign Keys
        public long QuizQuestionId { get; set; }
        public QuizQuestion QuizQuestion { get; set; } = null!;

        public long? MediaAssetId { get; set; }
        public MediaAsset? Media { get; set; }

        // Navigation for Many-to-Many Responses
        public ICollection<QuizQuestionResponse> Responses { get; set; } = new List<QuizQuestionResponse>();
    }
}