namespace Sqeez.Api.DTOs
{
    public record EnrollmentDto(
        long Id,
        int? Mark,
        DateTime EnrolledAt,
        DateTime? ArchivedAt,
        long StudentId,
        long SubjectId,
        int QuizAttemptsCount
        );

    public class EnrollmentFilterDto : PagedFilterDto
    {
        public int? Mark { get; set; }
        public long? StudentId { get; set; }
        public long? SubjectId { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDescending { get; set; } = false;
    }

    // Create handled by Task<ServiceResult<bool>> EnrollStudentsInSubjectAsync(long subjectId, AssignStudentsDto dto) in EnrollmentService

    public record PatchEnrollmentDto(
        int? Mark = null,
        bool? RemoveMark = null
    );
}
