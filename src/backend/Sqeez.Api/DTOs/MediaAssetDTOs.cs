using Sqeez.Api.Enums;

namespace Sqeez.Api.DTOs
{
    public record MediaAssetDto(
        long Id,
        string LocationUrl,
        MediaType MimeType,
        bool IsPrivate,
        string? Description,
        long OwnerId,
        string? OwnerUsername);

    public class MediaAssetFilterDto : PagedFilterDto
    {
        public string? SearchTerm { get; set; } // Search Description or LocationUrl
        public MediaType? MimeType { get; set; }
        public bool? IsPrivate { get; set; }
        public long? OwnerId { get; set; }
    }

    public record CreateMediaAssetDto(
        string LocationUrl,
        MediaType MimeType,
        bool IsPrivate,
        long OwnerId,
        string? Description = null);

    public record PatchMediaAssetDto(
        string? LocationUrl = null,
        MediaType? MimeType = null,
        bool? IsPrivate = null,
        string? Description = null);

    public class UploadMediaAssetDto
    {
        public IFormFile File { get; set; } = null!;
        public long OwnerId { get; set; }
        public bool IsPrivate { get; set; } = false;
        public string? Description { get; set; }
    }

    public record MediaDownloadDto(string LocationUrl, string MimeType);
}