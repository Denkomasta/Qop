namespace Sqeez.Api.Models.System
{
    public class SystemConfig
    {
        public int Id { get; set; }

        // --- BRANDING & UI ---
        public string SchoolName { get; set; } = "Sqeez";
        public string LogoUrl { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = "support@sqeez.com";
        public string DefaultLanguage { get; set; } = "en";

        // --- ACADEMICS ---
        public string CurrentAcademicYear { get; set; } = "2025/2026";

        // --- SECURITY & REGISTRATION ---
        public bool AllowPublicRegistration { get; set; } = false; // Usually false for schools; admins invite students
        public bool RequireEmailVerification { get; set; } = true;

        // --- TECHNICAL LIMITS ---
        public int MaxFileUploadSizeMB { get; set; } = 10; // Prevent students from uploading 4K videos
        public int MaxActiveSessionsPerUser { get; set; } = 3; // Prevent account sharing
    }
}