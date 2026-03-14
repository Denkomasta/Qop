using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ApiBaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpPost("me/avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var userIdClaim = GetUserIdFromClaims();
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            long userId = long.Parse(userIdClaim);

            var result = await _userService.UploadAvatarAsync(userId, file);

            if (!result.Success)
            {
                return HandleServiceResult(result);
            }

            return Ok(new
            {
                message = "Avatar updated successfully.",
                avatarUrl = result.Data
            });
        }
    }
}