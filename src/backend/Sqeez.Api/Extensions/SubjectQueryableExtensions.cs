using Sqeez.Api.Models.Academics;

namespace Sqeez.Api.Extensions
{
    public static class SubjectQueryableExtensions
    {
        /// <summary>
        /// Filters Subjects to only include those that have started and have not yet ended.
        /// </summary>
        public static IQueryable<Subject> WhereIsActive(this IQueryable<Subject> query)
        {
            var now = DateTime.UtcNow;
            return query.Where(s => s.StartDate <= now &&
                                   (!s.EndDate.HasValue || s.EndDate >= now));
        }

        /// <summary>
        /// Filters Subjects to only include those that are scheduled for the future or have already ended.
        /// </summary>
        public static IQueryable<Subject> WhereIsInactive(this IQueryable<Subject> query)
        {
            var now = DateTime.UtcNow;
            return query.Where(s => s.StartDate > now ||
                                   (s.EndDate.HasValue && s.EndDate < now));
        }
    }
}
