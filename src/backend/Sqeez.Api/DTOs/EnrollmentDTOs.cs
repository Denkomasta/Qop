using Sqeez.Api.Constants;
using System.ComponentModel.DataAnnotations;

namespace Sqeez.Api.DTOs
{
    public record EnrollmentDto(
        long Id,
        int? Mark,
        DateTime EnrolledAt,
        DateTime? ArchivedAt,
        long StudentId,
        string StudentUserName,
        long SubjectId,
        string SubjectName,
        string SubjectCode,
        int QuizAttemptsCount
        );

    public class EnrollmentFilterDto : PagedFilterDto
    {
        [Range(ValidationConstants.MinMark, ValidationConstants.MaxMark)]
        public int? Mark { get; set; }
        public long? StudentId { get; set; }
        public long? SubjectId { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDescending { get; set; } = false;
    }

    // Create handled by Task<ServiceResult<bool>> EnrollStudentsInSubjectAsync(long subjectId, AssignStudentsDto dto) in EnrollmentService

    public record PatchEnrollmentDto
    {
        public PatchEnrollmentDto() { }

        public PatchEnrollmentDto(int? Mark = null, bool? RemoveMark = null)
        {
            this.Mark = Mark;
            this.RemoveMark = RemoveMark;
        }

        [Range(ValidationConstants.MinMark, ValidationConstants.MaxMark)]
        public int? Mark { get; init; }
        public bool? RemoveMark { get; init; }
    }

    public record EnrollmentBasicDto
    {
        public long Id { get; init; }
        public long SubjectId { get; init; }
        public string SubjectName { get; init; } = string.Empty;
        public int? Mark { get; init; }
        public DateTime EnrolledAt { get; init; }
        public DateTime? ArchivedAt { get; init; }
    }

    public class BulkEnrollmentResultDto
    {
        public List<long> NewlyEnrolledIds { get; set; } = new();
        public List<long> AlreadyEnrolledIds { get; set; } = new();
    }
}
