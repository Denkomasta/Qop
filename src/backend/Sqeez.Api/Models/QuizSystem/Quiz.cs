using Sqeez.Api.Models.Academics;

namespace Sqeez.Api.Models.QuizSystem
{
    public class Quiz
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxRetries { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime? ClosingDate { get; set; }

        // Foreign Key
        public long SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        // Navigation Properties
        public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
        public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    }
}