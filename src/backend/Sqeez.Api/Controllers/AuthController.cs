using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        private void SetTokens(AuthResponseDto tokens, bool rememberMe)
        {
            // Access Token Cookie (Always short-lived)
            var accessOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };
            Response.Cookies.Append("sqeez_access_token", tokens.AccessToken, accessOptions);

            // Refresh Token Cookie
            var refreshOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/api/auth/refresh"
            };

            if (rememberMe)
            {
                refreshOptions.Expires = DateTime.UtcNow.AddDays(7);
            }
            // If false, we leave expires null. The browser treats it as a "Session Cookie" 
            Response.Cookies.Append("sqeez_refresh_token", tokens.RefreshToken, refreshOptions);
        }

        private void ClearTokens()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Delete("sqeez_access_token", cookieOptions);
            Response.Cookies.Delete("sqeez_refresh_token", cookieOptions);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDTO registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Success) return HandleServiceResult(result);

            return Ok(new { message = "Registration successful. Please check your email to verify your account." });
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult> VerifyEmail([FromQuery] string token, [FromQuery] bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required." });

            var result = await _authService.VerifyEmailAsync(token, rememberMe);

            if (!result.Success || result.Data == null) return HandleServiceResult(result);

            SetTokens(result.Data, rememberMe);

            return Ok(new { message = "Email verified successfully. You are now logged in." });
        }

        [HttpPost("resend-verification")]
        public async Task<ActionResult> ResendVerificationEmail([FromBody] ResendVerificationDto dto)
        {
            var result = await _authService.ResendVerificationEmailAsync(dto);

            if (!result.Success)
                return HandleServiceResult(result);

            return Ok(new { message = "If an account with that email exists and is unverified, a new link has been sent." });
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDTO loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success || result.Data == null) return HandleServiceResult(result);

            SetTokens(result.Data, loginDto.RememberMe);

            return Ok(new { message = "Login successful" });
        }

        [HttpPost("refresh")]
        public async Task<ActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["sqeez_refresh_token"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { message = "No refresh token found. Please log in again." });
            }

            var result = await _authService.RefreshTokenAsync(new RefreshTokenDto(refreshToken));

            if (!result.Success || result.Data == null)
            {
                ClearTokens();
                return HandleServiceResult(result);
            }

            SetTokens(result.Data, true);

            return Ok(new { message = "Session refreshed successfully." });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            var userIdClaim = GetUserIdFromClaims();

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            var currentRefreshToken = Request.Cookies["sqeez_refresh_token"];

            var result = await _authService.LogoutAsync(long.Parse(userIdClaim), currentRefreshToken);

            if (!result.Success)
                return HandleServiceResult(result);

            ClearTokens();

            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize]
        [HttpGet("me")]
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
        public async Task<ActionResult> ElevateUser(UpdateRoleDTO dto)
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