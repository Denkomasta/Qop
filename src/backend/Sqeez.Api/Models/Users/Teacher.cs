using Sqeez.Api.Models.Academics;
using Sqeez.Api.Models.Media;

namespace Sqeez.Api.Models.Users
{
    public class Teacher : Student
    {
        public string? Department { get; set; }

        // Navigation Properties
        public SchoolClass? ManagedClass { get; set; } // The class they teach
        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();
    }
}