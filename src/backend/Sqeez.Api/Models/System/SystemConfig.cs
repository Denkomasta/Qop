namespace Sqeez.Api.Models.System
{
    public class SystemConfig
    {
        public int Id { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public int CurrentAcademicYear { get; set; }
        public bool AllowPublicRegistration { get; set; }
        public string DefaultLanguage { get; set; } = "en";
        public int MaxFileUploadSizeMB { get; set; }
    }
}