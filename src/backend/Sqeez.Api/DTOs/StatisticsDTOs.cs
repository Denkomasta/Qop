namespace Sqeez.Api.DTOs
{
    public class QuizSummaryStatDto
    {
        public long QuizId { get; set; }
        public int TotalAttempts { get; set; }
        public int CompletedAttempts { get; set; }
        public double AverageScore { get; set; }
        public int HighestScore { get; set; }
        public int LowestScore { get; set; }
        public double AverageCompletionTimeMinutes { get; set; }
    }

    public class OptionStatDto
    {
        public long Id { get; set; }
        public string? Text { get; set; }
        public int PickCount { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuestionStatDto
    {
        public long Id { get; set; }
        public string? QuestionText { get; set; }
        public bool IsFreeText { get; set; }
        public int TotalAnswers { get; set; }
        public double AverageScore { get; set; }
        public double AverageResponseTimeSeconds { get; set; }
        public List<OptionStatDto> Options { get; set; } = new();
        public List<string> SubmittedFreeTextAnswers { get; set; } = new();
    }
}
