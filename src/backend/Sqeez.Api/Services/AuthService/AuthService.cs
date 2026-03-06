using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Models.Users;
using BC = BCrypt.Net.BCrypt;
using Sqeez.Api.Services.TokenService;

namespace Sqeez.Api.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly SqeezDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(SqeezDbContext context, ITokenService tokenService, ILogger<AuthService> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<string?> LoginAsync(LoginDTO dto)
        {
            _logger.LogInformation("Attempting to login user: {Email}", dto.Email);
            var user = await _context.Students
                .FirstOrDefaultAsync(u => u.Email == dto.Email.Trim().ToLower());

            if (user == null) return null;

            bool isValid = BC.Verify(dto.Password.Trim(), user.PasswordHash);

            if (!isValid) return null;

            user.IsOnline = true;
            user.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _tokenService.CreateToken(user);
        }

        public async Task<string?> RegisterAsync(RegisterDTO dto)
        {
            _logger.LogInformation("Attempting to register user: {Email}", dto.Email);

            string email = dto.Email.Trim().ToLower();
            if (await _context.Students.AnyAsync(x => x.Email == email))
                return null;

            string salt = BC.GenerateSalt(12);
            string hashedPassword = BC.HashPassword(dto.Password.Trim(), salt);
            string username = string.IsNullOrWhiteSpace(dto.Username) ? email.Split('@')[0] : dto.Username;

            var user = new Student
            {
                Username = username,
                Email = email,
                PasswordHash = hashedPassword,
                Role = Enums.UserRole.Student,
                LastSeen = DateTime.UtcNow,
            };

            _context.Students.Add(user);

            await _context.SaveChangesAsync();

            return _tokenService.CreateToken(user);
        }

        public async Task<bool> LogoutAsync(long userId)
        {
            _logger.LogInformation("Attempting to logout user: {id}", userId);

            var user = await _context.Students.FindAsync(userId);

            if (user == null) return false;

            user.IsOnline = false;
            user.LastSeen = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserDTO?> GetCurrentUserAsync(long userId, string? role)
        {
            var user = role switch
            {
                "Teacher" => await _context.Teachers.FindAsync(userId),
                "Admin" => await _context.Admins.FindAsync(userId),
                _ => await _context.Students.FindAsync(userId),
            };

            if (user == null) return null;
            
            return new UserDTO(
                Id: user.Id,
                Username: user.Username,
                Email: user.Email,
                CurrentXP: user.CurrentXP.ToString(),
                Role: user.Role.ToString(),
                Department: user is Teacher t ? t.Department : null,
                PhoneNumber: user is Admin a ? a.PhoneNumber : null
            );
        }
    }
}