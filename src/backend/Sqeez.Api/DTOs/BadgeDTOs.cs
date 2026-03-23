using Sqeez.Api.Enums;

namespace Sqeez.Api.DTOs
{
    public record BadgeRuleDto(
        long Id,
        BadgeMetric Metric,
        BadgeOperator Operator,
        decimal TargetValue
    );

    public record CreateBadgeRuleDto(
        BadgeMetric Metric,
        BadgeOperator Operator,
        decimal TargetValue
    );

    public record UpdateBadgeRuleDto(
        long? Id, // Null means "Create a new rule attached to this badge"
        BadgeMetric Metric,
        BadgeOperator Operator,
        decimal TargetValue
    );

    // Using classes here because model binding IFormFile with Lists in records can be tricky in ASP.NET
    public class CreateBadgeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int XpBonus { get; set; }
        public IFormFile? IconFile { get; set; } = null;

        public List<CreateBadgeRuleDto> Rules { get; set; } = new List<CreateBadgeRuleDto>();
    }

    public class UpdateBadgeDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? XpBonus { get; set; }
        public IFormFile? NewIconFile { get; set; }
        public List<UpdateBadgeRuleDto>? Rules { get; set; }
    }

    public record BadgeDto(
        long Id,
        string Name,
        string Description,
        string? IconUrl,
        int XpBonus,
        List<BadgeRuleDto> Rules
    );

    public record StudentBadgeDto(
        long BadgeId,
        string Name,
        string Description,
        string? IconUrl,
        int XpBonus,
        DateTime EarnedAt
    );

    public record StudentBadgeBasicDto
    {
        public long BadgeId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? IconUrl { get; init; }
        public DateTime EarnedAt { get; init; }
    }

    public record BadgeEvaluationMetrics(
        decimal ScorePercentage,
        int TotalScore,
        int PerfectAnswersCount,
        int TotalAttempts
    );

    public class BadgeFilterDto : PagedFilterDto
    {
        public string? SearchTerm { get; init; }
        public bool? isEarned { get; init; }
        public long? StudentId { get; init; }
    }
}