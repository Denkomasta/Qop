using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface IQuizAttemptService
    {
        /// <summary>
        /// Creates a new QuizAttempt record and sets the status to Started.
        /// </summary>
        Task<ServiceResult<QuizAttemptDto>> StartAttemptAsync(long studentId, StartQuizAttemptDto dto);

        /// <summary>
        /// Saves or updates a student's answer for a single question during an active attempt.
        /// </summary>
        Task<ServiceResult<QuestionAnsweredDto>> SubmitAnswerAsync(long attemptId, long studentId, SubmitQuestionResponseDto dto);

        /// <summary>
        /// Returns the next upcoming question that should be answered in started quiz.
        /// </summary>
        Task<ServiceResult<long?>> GetNextPendingQuestionIdAsync(long attemptId, long studentId);

        /// <summary>
        /// Locks the attempt, calculates the final score, and sets the status to Completed.
        /// </summary>
        Task<ServiceResult<QuizAttemptDto>> CompleteAttemptAsync(long attemptId, long studentId);

        /// <summary>
        /// Gets the full details of an attempt, including all submitted answers.
        /// (Students can view their own; Teachers/Admins can view anyone's).
        /// </summary>
        Task<ServiceResult<QuizAttemptDetailDto>> GetAttemptDetailsAsync(long attemptId, long currentUserId, string currentUserRole);

        /// <summary>
        /// Gets a paginated list of all attempts for a specific quiz (Used by Teachers to see the class results).
        /// </summary>
        Task<ServiceResult<PagedResponse<QuizAttemptDto>>> GetAttemptsForQuizAsync(long quizId, long teacherId, int pageNumber = 1, int pageSize = 20);


        // --- TEACHER ACTIONS ---

        /// <summary>
        /// Allows a teacher to manually grade a free-text question, update the score, and optionally "Like" the answer.
        /// </summary>
        Task<ServiceResult<QuestionResponseDto>> GradeFreeTextResponseAsync(long responseId, long teacherId, GradeQuestionResponseDto dto);

        /// <summary>
        /// Allows a teacher to manually reset (delete) student's attempt.
        /// </summary>
        Task<ServiceResult<bool>> DeleteAttemptAsync(long attemptId, long teacherId);
    }
}