using Sqeez.Api.Enums;

namespace Sqeez.Api.DTOs
{
    public record BadgeRuleDto(
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

    public record UpdateBadgeDto(
        string Name,
        string Description,
        string IconUrl,
        int XpBonus,
        List<BadgeRuleDto> Rules
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