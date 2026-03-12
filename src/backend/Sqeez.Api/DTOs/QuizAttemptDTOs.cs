using Sqeez.Api.Enums;

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
        long? NextQuestionId = null
    );

    public record QuestionResponseDto(
        long Id,
        long QuizQuestionId,
        long ResponseTimeMs,
        string? FreeTextAnswer,
        bool IsLiked,
        int Score,
        List<long> SelectedOptionIds
    );

    public record QuestionAnsweredDto(
        long Id,
        long QuizQuestionId,
        long ResponseTimeMs,
        string? FreeTextAnswer,
        bool IsLiked,
        int Score,
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

    public record SubmitQuestionResponseDto(
        long QuizQuestionId,
        long ResponseTimeMs,
        string? FreeTextAnswer,
        List<long> SelectedOptionIds
    );

    public record GradeQuestionResponseDto(
        int Score,
        bool IsLiked
    );
}