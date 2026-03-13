namespace Sqeez.Api.DTOs
{
    public record CreateBadgeDto(
        string Name,
        string Description,
        string IconUrl,
        int XpBonus,
        string Condition
    );

    public record UpdateBadgeDto(
        string Name,
        string Description,
        string IconUrl,
        int XpBonus,
        string Condition
    );

    public record BadgeDto(
        long Id,
        string Name,
        string Description,
        string IconUrl,
        int XpBonus,
        string Condition
    );

    public record StudentBadgeDto(
        long BadgeId,
        string Name,
        string Description,
        string IconUrl,
        int XpBonus,
        DateTime EarnedAt
    );
}