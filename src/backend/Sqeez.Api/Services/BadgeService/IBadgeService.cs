using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface IBadgeService
    {
        Task<ServiceResult<BadgeDto>> CreateBadgeAsync(CreateBadgeDto dto);
        Task<ServiceResult<BadgeDto>> UpdateBadgeAsync(long id, UpdateBadgeDto dto);
        Task<ServiceResult<bool>> DeleteBadgeAsync(long id);
        Task<ServiceResult<IEnumerable<BadgeDto>>> GetAllBadgesAsync();

        Task<ServiceResult<IEnumerable<StudentBadgeDto>>> GetStudentBadgesAsync(long studentId);
        Task<ServiceResult<bool>> AwardBadgeToStudentAsync(long studentId, long badgeId);
    }
}