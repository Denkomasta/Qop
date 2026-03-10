namespace Sqeez.Api.DTOs
{
    public record QuizDto(
        long Id,
        string Title,
        string Description,
        int MaxRetries,
        DateTime CreatedAt,
        DateTime? PublishDate,
        DateTime? ClosingDate,
        long SubjectId,
        int QuizQuestions,
        int QuizAttempts);

    public class QuizFilterDto : PagedFilterDto
    {
        public string? SearchTerm { get; set; }  // Search against Title or Description
        public bool? IsActive { get; set; }
        public DateTime? PublishDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public long? SubjectId { get; set; }
    }

    public record CreateQuizDto(
        string Title,
        string Description,
        long SubjectId,
        int MaxRetries = 0,
        DateTime? PublishDate = null,
        DateTime? ClosingDate = null);

    public record PatchQuizDto(
        string? Title = null,
        string? Description = null,
        int? MaxRetries = null,
        long? SubjectId = null,
        DateTime? PublishDate = null,
        DateTime? ClosingDate = null
        );

    public record QuizQuestionDto(
        long Id,
        string Title,
        int Difficulty,
        int TimeLimit,
        long QuizId,
        long? MediaAssetId,
        int QuizOptions);

    public class QuizQuestionFilterDto : PagedFilterDto
    {
        public string? SearchTerm { get; set; }  // Search against Title
        public int? Difficulty { get; set; }
        public long? QuizId { get; set; }
        public long? MediaAssetId { get; set; }
    }

    public record CreateQuizQuestionDto(
        string Title,
        int Difficulty,
        int TimeLimit,
        long QuizId,
        long? MediaAssetId = null);

    public record PatchQuizQuestionDto(
        string? Title = null,
        int? Difficulty = null,
        int? TimeLimit = null,
        long? MediaAssetId = null
        );

    public record QuizOptionDto(
        long Id,
        string? Text,
        bool IsFreeText,
        bool IsCorrect,
        long QuizQuestionId,
        long? MediaAssetId,
        int Responses);

    public class QuizOptionFilterDto : PagedFilterDto
    {
        public string? SearchTerm { get; set; }  // Search against Text
        public bool? IsFreeText { get; set; }
        public bool? IsCorrect { get; set; }
        public long? QuizQuestionId { get; set; }
        public long? MediaAssetId { get; set; }
    }

    public record CreateQuizOptionDto(
        bool IsCorrect,
        long QuizQuestionID,
        string? Text = null,
        bool IsFreeText = false,
        long? MediaAssetId = null);

    public record PatchQuizOptionDto(
        string? Text = null,
        bool? IsFreeText = null,
        bool? IsCorrect = null,
        long? MediaAssetId = null
        );
}
