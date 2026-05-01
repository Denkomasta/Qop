using Sqeez.Api.Constants;
using System.ComponentModel.DataAnnotations;

namespace Sqeez.Api.DTOs
{
    public record SubjectDto(
        long Id,
        string Name,
        string Code,
        string? Description,
        DateTime StartDate,
        DateTime? EndDate,
        long? TeacherId,
        string? TeacherName,
        long? SchoolClassId,
        string? SchoolClassName,
        int EnrollmentCount,    // Enrollements are added in Enrollment service
        int QuizCount           // Quizzes are added in Quiz service
        );

    public class SubjectFilterDto : PagedFilterDto
    {
        [StringLength(ValidationConstants.SearchTermMaxLength)]
        public string? SearchTerm { get; set; }

        [StringLength(ValidationConstants.SubjectCodeMaxLength)]
        public string? Code { get; set; }

        public long? TeacherId { get; set; }
        public long? SchoolClassId { get; set; }
        public long? StudentId { get; set; }

        public bool? IsActive { get; set; }
        public DateTime? StartingAfter { get; set; }

        public bool IsDescending { get; set; } = false;
    }

    public record CreateSubjectDto
    {
        public CreateSubjectDto() { }

        public CreateSubjectDto(string Name, string Code, string? Description = null, DateTime? StartDate = null, DateTime? EndDate = null, long? TeacherId = null, long? SchoolClassId = null)
        {
            this.Name = Name;
            this.Code = Code;
            this.Description = Description;
            this.StartDate = StartDate;
            this.EndDate = EndDate;
            this.TeacherId = TeacherId;
            this.SchoolClassId = SchoolClassId;
        }

        [StringLength(ValidationConstants.TitleMaxLength)]
        public string Name { get; init; } = string.Empty;

        [StringLength(ValidationConstants.SubjectCodeMaxLength)]
        public string Code { get; init; } = string.Empty;

        [StringLength(ValidationConstants.DescriptionMaxLength)]
        public string? Description { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public long? TeacherId { get; init; }
        public long? SchoolClassId { get; init; }
    }

    public record PatchSubjectDto
    {
        public PatchSubjectDto() { }

        public PatchSubjectDto(string? Name = null, string? Code = null, string? Description = null, DateTime? StartDate = null, DateTime? EndDate = null, long? TeacherId = null, long? SchoolClassId = null)
        {
            this.Name = Name;
            this.Code = Code;
            this.Description = Description;
            this.StartDate = StartDate;
            this.EndDate = EndDate;
            this.TeacherId = TeacherId;
            this.SchoolClassId = SchoolClassId;
        }

        [StringLength(ValidationConstants.TitleMaxLength)]
        public string? Name { get; init; }

        [StringLength(ValidationConstants.SubjectCodeMaxLength)]
        public string? Code { get; init; }

        [StringLength(ValidationConstants.DescriptionMaxLength)]
        public string? Description { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public long? TeacherId { get; init; }     // Pass 0 to remove the teacher
        public long? SchoolClassId { get; init; } // Pass 0 to remove the class
    }

    public record SubjectBasicDto
    {
        public long Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
    }
}
