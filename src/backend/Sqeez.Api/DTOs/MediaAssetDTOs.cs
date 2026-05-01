using Sqeez.Api.Constants;
using Sqeez.Api.Enums;
using System.ComponentModel.DataAnnotations;

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
        [StringLength(ValidationConstants.SearchTermMaxLength)]
        public string? SearchTerm { get; set; } // Search Description or LocationUrl
        public MediaType? MimeType { get; set; }
        public bool? IsPrivate { get; set; }
        public long? OwnerId { get; set; }
    }

    public record CreateMediaAssetDto
    {
        public CreateMediaAssetDto() { }

        public CreateMediaAssetDto(string LocationUrl, MediaType MimeType, bool IsPrivate, long OwnerId, string? Description = null)
        {
            this.LocationUrl = LocationUrl;
            this.MimeType = MimeType;
            this.IsPrivate = IsPrivate;
            this.OwnerId = OwnerId;
            this.Description = Description;
        }

        [StringLength(ValidationConstants.UrlMaxLength)]
        public string LocationUrl { get; init; } = string.Empty;
        public MediaType MimeType { get; init; }
        public bool IsPrivate { get; init; }
        public long OwnerId { get; init; }

        [StringLength(ValidationConstants.DescriptionMaxLength)]
        public string? Description { get; init; }
    }

    public record PatchMediaAssetDto
    {
        public PatchMediaAssetDto() { }

        public PatchMediaAssetDto(string? LocationUrl = null, MediaType? MimeType = null, bool? IsPrivate = null, string? Description = null)
        {
            this.LocationUrl = LocationUrl;
            this.MimeType = MimeType;
            this.IsPrivate = IsPrivate;
            this.Description = Description;
        }

        [StringLength(ValidationConstants.UrlMaxLength)]
        public string? LocationUrl { get; init; }
        public MediaType? MimeType { get; init; }
        public bool? IsPrivate { get; init; }

        [StringLength(ValidationConstants.DescriptionMaxLength)]
        public string? Description { get; init; }
    }

    public class UploadMediaAssetDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
        public long OwnerId { get; set; }
        public bool IsPrivate { get; set; } = false;
        [StringLength(ValidationConstants.DescriptionMaxLength)]
        public string? Description { get; set; }
    }

    public record MediaDownloadDto(string LocationUrl, string MimeType);
}
