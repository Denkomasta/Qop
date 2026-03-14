namespace Sqeez.Api.Models.Users
{
    public class UserSession
    {
        public long Id { get; set; }

        // The Foreign Key
        public long UserId { get; set; }
        public Student User { get; set; } = null!;

        public string RefreshToken { get; set; } = string.Empty;
        //public string DeviceInfo { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
    }
}