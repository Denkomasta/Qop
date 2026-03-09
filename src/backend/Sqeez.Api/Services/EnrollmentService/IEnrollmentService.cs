//using Sqeez.Api.DTOs;

//namespace Sqeez.Api.Services.Interfaces
//{
//    public interface IEnrollmentService
//    {
//        Task<ServiceResult<bool>> EnrollStudentsInSubjectAsync(long subjectId, EnrollStudentsDto dto);
//        Task<ServiceResult<bool>> UnenrollStudentsFromSubjectAsync(long subjectId, UnenrollStudentsDto dto);

//        Task<ServiceResult<PagedResponse<EnrollmentDto>>> GetSubjectEnrollmentsAsync(long subjectId, EnrollmentFilterDto filter);
//        Task<ServiceResult<IEnumerable<EnrollmentDto>>> GetStudentEnrollmentsAsync(long studentId);

//        Task<ServiceResult<EnrollmentDto>> GetEnrollmentByIdAsync(long id);
//        Task<ServiceResult<bool>> UpdateEnrollmentMarkAsync(long enrollmentId, int newMark);
//        Task<ServiceResult<bool>> ToggleEnrollmentStatusAsync(long enrollmentId, bool isActive);
//    }
//}