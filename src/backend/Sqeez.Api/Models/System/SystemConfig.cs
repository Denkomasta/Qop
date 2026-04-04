namespace Sqeez.Api.Models.System
{
    public class SystemConfig
    {
        public int Id { get; set; }

        // --- BRANDING & UI ---
        public string SchoolName { get; set; } = "Sqeez";
        public string LogoUrl { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = "support@sqeez.org";
        public string DefaultLanguage { get; set; } = "en";

        // --- ACADEMICS ---
        public string CurrentAcademicYear { get; set; } = "2025/2026";

        // --- SECURITY & REGISTRATION ---
        public bool AllowPublicRegistration { get; set; } = false; // Usually false for schools; admins invite students
        public bool RequireEmailVerification { get; set; } = true;

        // --- TECHNICAL LIMITS ---
        public int MaxAvatarAndBadgeUploadSizeMB { get; set; } = 5; // Images only (5MB is plenty for high-res web images)
        public int MaxQuizMediaUploadSizeMB { get; set; } = 50; // Videos, Audio, complex diagrams (50MB)
        public int MaxActiveSessionsPerUser { get; set; } = 3; // Prevent account sharing
    }
}