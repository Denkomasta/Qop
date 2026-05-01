using Sqeez.Api.Constants;
using Sqeez.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sqeez.Api.DTOs
{
    public record QuizAttemptDto(
        long Id,
        long QuizId,
        long EnrollmentId,
        DateTime? StartTime,
        DateTime? EndTime,
        AttemptStatus Status,
        int TotalScore,
        int? Mark,
        long? NextQuestionId = null,
        List<StudentBadgeBasicDto>? EarnedBadges = null,
        string? StudentName = null,
        long? StudentId = null
    );

    public record QuestionResponseDto(
        long Id,
        long QuizQuestionId,
        long ResponseTimeMs,
        string? FreeTextAnswer,
        bool IsLiked,
        int? Score,
        List<long> SelectedOptionIds
    );

    public record QuestionAnsweredDto(
        long Id,
        long QuizQuestionId,
        long ResponseTimeMs,
        string? FreeTextAnswer,
        bool IsLiked,
        int? Score,
        List<long> SelectedOptionIds,
        List<long>? CorrectOptionIds = null,
        string? CorrectFreeTextAnswer = null,
        long? NextQuestionId = null
    );

    public record QuizAttemptDetailDto(
        long Id,
        long QuizId,
        long EnrollmentId,
        DateTime? StartTime,
        DateTime? EndTime,
        AttemptStatus Status,
        int TotalScore,
        int? Mark,
        List<QuestionResponseDto> Responses
    );

    public record StartQuizAttemptDto(
        long QuizId,
        long EnrollmentId
    );

    public record SubmitQuestionResponseDto
    {
        public SubmitQuestionResponseDto() { }

        public SubmitQuestionResponseDto(long QuizQuestionId, long ResponseTimeMs, string? FreeTextAnswer, List<long> SelectedOptionIds)
        {
            this.QuizQuestionId = QuizQuestionId;
            this.ResponseTimeMs = ResponseTimeMs;
            this.FreeTextAnswer = FreeTextAnswer;
            this.SelectedOptionIds = SelectedOptionIds;
        }

        public long QuizQuestionId { get; init; }

        [Range(0, ValidationConstants.MaxResponseTimeMs)]
        public long ResponseTimeMs { get; init; }

        [StringLength(ValidationConstants.LongTextMaxLength)]
        public string? FreeTextAnswer { get; init; }

        [MaxLength(ValidationConstants.MaxBulkIds)]
        public List<long> SelectedOptionIds { get; init; } = new();
    }

    public record GradeQuestionResponseDto
    {
        public GradeQuestionResponseDto() { }

        public GradeQuestionResponseDto(int Score, bool IsLiked)
        {
            this.Score = Score;
            this.IsLiked = IsLiked;
        }

        [Range(-ValidationConstants.MaxQuestionDifficulty, ValidationConstants.MaxQuestionDifficulty)]
        public int Score { get; init; }
        public bool IsLiked { get; init; }
    }
}
