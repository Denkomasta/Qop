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
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        private string? GetUserIdFromClaims()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        private string? GetUserRoleFromClaims()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
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
            var token = await _authService.RegisterAsync(registerDto);

            if (token == null) return BadRequest("Email already in use.");

            SetCookie(token);

            return Ok(new { message = "Registration was successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            var token = await _authService.LoginAsync(loginDto);

            if (token == null) return Unauthorized("Invalid credentials.");

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

            var success = await _authService.LogoutAsync(long.Parse(userIdClaim));

            if (!success)
                return NotFound("User record not found.");

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

            var user = await _authService.GetCurrentUserAsync(long.Parse(userIdClaim), role);

            if (user == null)
                return NotFound("User record not found.");

            return Ok(user);
        }
    }
}