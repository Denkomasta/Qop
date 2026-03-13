using Sqeez.Api.Enums;

namespace Sqeez.Api.Models.Gamification
{
    public class BadgeRule
    {
        public long Id { get; set; }

        // Foreign Key to the Badge
        public long BadgeId { get; set; }
        public Badge Badge { get; set; } = null!;

        // The actual rule data
        public BadgeMetric Metric { get; set; }
        public BadgeOperator Operator { get; set; }
        public decimal TargetValue { get; set; }
    }
}