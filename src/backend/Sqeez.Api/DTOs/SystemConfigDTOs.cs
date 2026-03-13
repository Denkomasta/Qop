namespace Sqeez.Api.DTOs
{
    public record SystemConfigDto(
        string SchoolName,
        string LogoUrl,
        string SupportEmail,
        string DefaultLanguage,
        string CurrentAcademicYear,
        bool AllowPublicRegistration,
        bool RequireEmailVerification,
        int MaxFileUploadSizeMB,
        int MaxActiveSessionsPerUser
    );

    public record UpdateSystemConfigDto(
        string? SchoolName,
        string? LogoUrl,
        string? SupportEmail,
        string? DefaultLanguage,
        string? CurrentAcademicYear,
        bool? AllowPublicRegistration,
        bool? RequireEmailVerification,
        int? MaxFileUploadSizeMB,
        int? MaxActiveSessionsPerUser
    );
}