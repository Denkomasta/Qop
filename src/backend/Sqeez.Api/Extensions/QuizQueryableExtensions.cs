using Sqeez.Api.Models.QuizSystem;

namespace Sqeez.Api.Extensions
{
    public static class QuizQueryableExtensions
    {
        /// <summary>
        /// Filters Quizzes to only include those that are published, have not closed, 
        /// AND belong to a Subject that has not yet ended.
        /// </summary>
        public static IQueryable<Quiz> WhereIsActive(this IQueryable<Quiz> query)
        {
            var now = DateTime.UtcNow;
            return query.Where(q => q.PublishDate != null &&
                                    q.PublishDate <= now &&
                                    (q.ClosingDate == null || q.ClosingDate > now) &&
                                    (q.Subject.EndDate == null || q.Subject.EndDate >= now));
        }

        /// <summary>
        /// Filters Quizzes to include drafts, future scheduled quizzes, closed quizzes, 
        /// OR quizzes belonging to a Subject that has ended.
        /// </summary>
        public static IQueryable<Quiz> WhereIsInactive(this IQueryable<Quiz> query)
        {
            var now = DateTime.UtcNow;
            return query.Where(q => q.PublishDate == null ||
                                    q.PublishDate > now ||
                                    (q.ClosingDate != null && q.ClosingDate <= now) ||
                                    (q.Subject.EndDate != null && q.Subject.EndDate < now));
        }
    }
}
