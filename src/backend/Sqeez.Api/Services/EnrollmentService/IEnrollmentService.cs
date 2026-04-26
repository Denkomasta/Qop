using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface IEnrollmentService
    {
        Task<ServiceResult<PagedResponse<EnrollmentDto>>> GetAllEnrollmentsAsync(EnrollmentFilterDto filter);
        Task<ServiceResult<EnrollmentDto>> GetEnrollmentByIdAsync(long id);
        Task<ServiceResult<EnrollmentDto>> PatchEnrollmentAsync(long id, PatchEnrollmentDto enrollment, long currentUserId);
        Task<ServiceResult<bool>> DeleteEnrollmentAsync(long id);

        Task<ServiceResult<BulkEnrollmentResultDto>> EnrollStudentsInSubjectAsync(long subjectId, AssignStudentsDto dto);
        Task<ServiceResult<bool>> UnenrollStudentsFromSubjectAsync(long subjectId, RemoveStudentsDto dto);
    }
}