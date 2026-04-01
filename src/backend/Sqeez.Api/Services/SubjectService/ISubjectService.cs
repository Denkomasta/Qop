using Sqeez.Api.DTOs;
using Sqeez.Api.Models.Academics;
using Sqeez.Api.Models.Import;

namespace Sqeez.Api.Services.Interfaces
{
    public interface ISubjectService
    {
        Task<ServiceResult<PagedResponse<SubjectDto>>> GetAllSubjectsAsync(SubjectFilterDto filter);
        Task<ServiceResult<SubjectDto>> GetSubjectByIdAsync(long id);
        Task<ServiceResult<SubjectDto>> CreateSubjectAsync(CreateSubjectDto dto);
        Task<ServiceResult<BulkOperationResult<SubjectDto>>> CreateSubjectsBulkAsync(IEnumerable<Subject> subjects);
        Task<ServiceResult<SubjectDto>> PatchSubjectAsync(long id, PatchSubjectDto dto);
        Task<ServiceResult<bool>> DeleteSubjectAsync(long id);
    }
}