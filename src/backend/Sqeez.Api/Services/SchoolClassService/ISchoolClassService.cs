using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface ISchoolClassService
    {
        Task<ServiceResult<PagedResponse<SchoolClassDto>>> GetAllClassesAsync(SchoolClassFilterDto filter);

        Task<ServiceResult<SchoolClassDetailDto>> GetClassByIdAsync(long id);
        Task<ServiceResult<SchoolClassDto>> CreateClassAsync(CreateSchoolClassDto dto);
        Task<ServiceResult<SchoolClassDto>> PatchClassAsync(long id, PatchSchoolClassDto dto);
        Task<ServiceResult<bool>> DeleteClassAsync(long id);
        Task<ServiceResult<bool>> AssignStudentsToClassAsync(long classId, AssignStudentsDto dto);
        Task<ServiceResult<bool>> RemoveStudentsFromClassAsync(long classId, RemoveStudentsDto dto);
    }
}