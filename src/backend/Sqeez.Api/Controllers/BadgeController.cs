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
        public async Task<ActionResult> CreateBadge([FromForm] CreateBadgeDto dto)
        {
            var result = await _badgeService.CreateBadgeAsync(dto);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        public async Task<ActionResult> PatchBadge(long id, [FromForm] UpdateBadgeDto dto)
        {
            var result = await _badgeService.UpdateBadgeAsync(id, dto);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBadge(long id)
        {
            var result = await _badgeService.DeleteBadgeAsync(id);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{badgeId}/award/{studentId}")]
        public async Task<ActionResult> AwardBadge(long badgeId, long studentId)
        {
            var result = await _badgeService.AwardBadgeToStudentAsync(studentId, badgeId);
            return HandleServiceResult(result);
        }

        [HttpGet]
        public async Task<ActionResult> GetAllBadges()
        {
            var result = await _badgeService.GetAllBadgesAsync();
            return HandleServiceResult(result);
        }

        [HttpGet("my-badges")]
        public async Task<ActionResult> GetMyBadges()
        {
            var userIdStr = GetUserIdFromClaims();
            if (!long.TryParse(userIdStr, out long currentUserId))
                return Unauthorized("Invalid user ID token.");

            var result = await _badgeService.GetStudentBadgesAsync(currentUserId);
            return HandleServiceResult(result);
        }
    }
}