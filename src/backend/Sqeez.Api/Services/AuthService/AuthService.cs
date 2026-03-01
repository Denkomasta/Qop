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

        public AuthService(SqeezDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<string?> LoginAsync(LoginDTO loginDto)
        {
            var user = await _context.Students
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

            if (user == null) return null;

            bool isValid = BC.Verify(loginDto.Password, user.PasswordHash);

            if (!isValid) return null;

            user.IsOnline = true;
            user.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _tokenService.CreateToken(user);
        }

        public async Task<bool> RegisterAsync(StudentCreateDTO dto)
        {
            if (await _context.Students.AnyAsync(x => x.Email == dto.Email.ToLower()))
                return false;

            string salt = BC.GenerateSalt(12);
            string hashedPassword = BC.HashPassword(dto.Password, salt);

            var user = new Student
            {
                Username = dto.Username,
                Email = dto.Email.ToLower(),
                PasswordHash = hashedPassword,
                Role = Enums.UserRole.Student
            };

            _context.Students.Add(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> LogoutAsync(long userId)
        {
            var user = await _context.Students.FindAsync(userId);

            if (user == null) return false;

            user.IsOnline = false;
            user.LastSeen = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}