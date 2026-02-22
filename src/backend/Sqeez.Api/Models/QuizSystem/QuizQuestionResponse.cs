namespace Sqeez.Api.Models.QuizSystem
{
    public class QuizQuestionResponse
    {
        public long Id { get; set; }
        public long ResponseTimeMs { get; set; }
        public string? FreeTextAnswer { get; set; }
        public bool IsLiked { get; set; }
        public int Score { get; set; }

        // Foreign Keys
        public long QuizAttemptId { get; set; }
        public QuizAttempt QuizAttempt { get; set; } = null!;

        public long QuizQuestionId { get; set; }
        public QuizQuestion QuizQuestion { get; set; } = null!;

        // Navigation Property (Many-to-Many with QuizOption)
        public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
    }
}