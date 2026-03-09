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

    public record CreateEnrollmentDto(
        DateTime? EnrolledAt,
        long StudentId,
        long SubjectId);

    public record PatchEnrollmentDto(
        int? Mark
    );
}
