using Sqeez.Api.Enums;

namespace Sqeez.Api.DTOs
{
    public record BadgeRuleDto(
        long Id,
        BadgeMetric Metric,
        BadgeOperator Operator,
        decimal TargetValue
    );

    public record CreateBadgeDto(
        string Name,
        string Description,
        string IconUrl,
        int XpBonus,
        List<BadgeRuleDto> Rules
    );

    public record UpdateBadgeRuleDto(
        long? Id,
        BadgeMetric Metric,
        BadgeOperator Operator,
        decimal TargetValue
    );

    public record UpdateBadgeDto(
        string? Name,
        string? Description,
        string? IconUrl,
        int? XpBonus,
        List<UpdateBadgeRuleDto>? Rules
    );

    public record BadgeDto(
        long Id,
        string Name,
        string Description,
        string IconUrl,
        int XpBonus,
        List<BadgeRuleDto> Rules
    );

    public record StudentBadgeDto(
        long BadgeId,
        string Name,
        string Description,
        string IconUrl,
        int XpBonus,
        DateTime EarnedAt
    );

    public record BadgeEvaluationMetrics(
        decimal ScorePercentage,
        int TotalScore
    );
}