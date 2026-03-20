using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqeez.Api.DTOs;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Controllers
{
    [Authorize]
    [Route("api/badges")]
    public class BadgesController : ApiBaseController
    {
        private readonly IBadgeService _badgeService;

        public BadgesController(IBadgeService badgeService)
        {
            _badgeService = badgeService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<BadgeDto>> CreateBadge([FromForm] CreateBadgeDto dto)
        {
            var result = await _badgeService.CreateBadgeAsync(dto);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<BadgeDto>> PatchBadge(long id, [FromForm] UpdateBadgeDto dto)
        {
            var result = await _badgeService.UpdateBadgeAsync(id, dto);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteBadge(long id)
        {
            var result = await _badgeService.DeleteBadgeAsync(id);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{badgeId}/award/{studentId}")]
        public async Task<ActionResult<bool>> AwardBadge(long badgeId, long studentId)
        {
            var result = await _badgeService.AwardBadgeToStudentAsync(studentId, badgeId);
            return HandleServiceResult(result);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<BadgeDto>>> GetAllBadges([FromQuery] BadgeFilterDto filter)
        {
            var result = await _badgeService.GetAllBadgesAsync(filter);
            return HandleServiceResult(result);
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<StudentBadgeDto>>> GetStudentBadges(long studentId)
        {
            var result = await _badgeService.GetStudentBadgesAsync(studentId);
            return HandleServiceResult(result);
        }
    }
}