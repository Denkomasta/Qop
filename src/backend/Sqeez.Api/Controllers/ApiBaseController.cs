using Microsoft.AspNetCore.Mvc;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;

namespace Sqeez.Api.Controllers
{
    [ApiController]
    public class ApiBaseController : ControllerBase
    {
        protected long CurrentUserId
        {
            get
            {
                var idString = GetUserIdFromClaims();
                return long.TryParse(idString, out long id) ? id : 0;
            }
        }

        protected bool IsCurrentUserAdmin => GetUserRoleFromClaims() == "Admin";

        protected string? GetUserIdFromClaims()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        protected string? GetUserRoleFromClaims()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        }

        protected bool IsIdLoggedUser(long userId, long? claimedId = null)
        {
            if (claimedId.HasValue)
            {
                return userId == claimedId.Value;
            }

            var idString = GetUserIdFromClaims();

            if (long.TryParse(idString, out long parsedClaimedId))
            {
                return parsedClaimedId == userId;
            }

            return false;
        }

        protected ActionResult HandleServiceResult<T>(ServiceResult<T> result)
        {
            if (result.Success)
            {
                return result.Data == null ? NotFound() : Ok(result.Data);
            }

            return result.ErrorCode switch
            {
                ServiceError.NotFound => NotFound(new { error = result.ErrorMessage }),
                ServiceError.Conflict => Conflict(new { error = result.ErrorMessage }),
                ServiceError.Unauthorized => Unauthorized(new { error = result.ErrorMessage }),
                ServiceError.ValidationFailed => BadRequest(new { error = result.ErrorMessage }),
                ServiceError.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new { error = result.ErrorMessage }),
                ServiceError.InternalError => StatusCode(StatusCodes.Status500InternalServerError, new { error = result.ErrorMessage }),
                ServiceError.TooManyRequests => StatusCode(StatusCodes.Status429TooManyRequests, new { error = result.ErrorMessage }),
                _ => BadRequest(new { error = result.ErrorMessage })
            };
        }
    }
}
