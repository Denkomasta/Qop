using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Services.AuthService;

namespace Sqeez.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ApiBaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        private void SetCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append("sqeez_token", token, cookieOptions);
        }

        private void ClearCookie()
        {
            Response.Cookies.Delete("sqeez_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);
            var token = result.Data;

            if (!result.Success || token == null) return HandleServiceResult(result);

            SetCookie(token);

            return Ok(new { message = "Registration was successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            var token = result.Data;

            if (!result.Success || token == null) return HandleServiceResult(result);

            SetCookie(token);

            return Ok(new { message = "Login successful" });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = GetUserIdFromClaims();

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            var result = await _authService.LogoutAsync(long.Parse(userIdClaim));

            if (!result.Success)
                return HandleServiceResult(result);

            ClearCookie();

            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize]
        [HttpPost("me")]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var userIdClaim = GetUserIdFromClaims();
            var role = GetUserRoleFromClaims();

            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(role))
            {
                return Unauthorized();
            }

            var result = await _authService.GetCurrentUserAsync(long.Parse(userIdClaim), role);

            return HandleServiceResult<UserDTO>(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("elevate")]
        public async Task<IActionResult> ElevateUser(UpdateRoleDTO dto)
        {
            var adminIdClaim = GetUserIdFromClaims();
            if (string.IsNullOrEmpty(adminIdClaim))
            {
                return Unauthorized();
            }

            var result = await _authService.UpdateUserRoleAsync(long.Parse(adminIdClaim), dto);

            if (!result.Success)
            {
                return HandleServiceResult(result);
            }

            return Ok(new { message = $"User rights updated to {dto.Role} successfully." });
        }
    }
}