using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;

namespace Sqeez.Api.Models.Media
{
    public class MediaAsset
    {
        public long Id { get; set; }
        public string LocationUrl { get; set; } = string.Empty;
        public MediaType MimeType { get; set; }
        public bool IsPrivate { get; set; }
        public string? Description { get; set; }

        // Foreign Key
        public long OwnerId { get; set; }
        public Teacher Owner { get; set; } = null!;
    }
}