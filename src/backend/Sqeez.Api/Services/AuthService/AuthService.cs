using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services.TokenService;
using BC = BCrypt.Net.BCrypt;

namespace Sqeez.Api.Services.AuthService
{
    public class AuthService : BaseService<AuthService>, IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly string _superUserEmail;

        public AuthService(SqeezDbContext context, IConfiguration config, ITokenService tokenService, ILogger<AuthService> logger) : base(context, logger)
        {
            _tokenService = tokenService;
            _superUserEmail = config["SUPER_USER_EMAIL"]?.Trim().ToLower() ?? string.Empty;
        }

        public async Task<ServiceResult<string>> LoginAsync(LoginDTO dto)
        {
            _logger.LogInformation("Attempting to login user: {Email}", dto.Email);
            var user = await _context.Students
                .FirstOrDefaultAsync(u => u.Email == dto.Email.Trim().ToLower());

            if (user == null) return ServiceResult<string>.Failure("Invalid email or password.", ServiceError.NotFound);

            bool isValid = BC.Verify(dto.Password.Trim(), user.PasswordHash);

            if (!isValid) return ServiceResult<string>.Failure("Invalid email or password.", ServiceError.Unauthorized);

            user.IsOnline = true;
            user.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _tokenService.CreateToken(user);
        }

        public async Task<ServiceResult<string>> RegisterAsync(RegisterDTO dto)
        {
            _logger.LogInformation("Attempting to register user: {Email}", dto.Email);

            string email = dto.Email.Trim().ToLower();
            string salt = BC.GenerateSalt(12);
            string hashedPassword = BC.HashPassword(dto.Password.Trim(), salt);
            string username = string.IsNullOrWhiteSpace(dto.Username) ? email.Split('@')[0] : dto.Username.Trim();

            // TODO rework to one query if performance is bad.
            if (await _context.Students.AnyAsync(x => x.Email == email))
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists.", email);
                return ServiceResult<string>.Failure("Email already exists.", ServiceError.Conflict);
            }

            if (await _context.Students.AnyAsync(x => x.Username == username))
            {
                _logger.LogWarning("Registration failed: Username {Username} already exists.", username);
                return ServiceResult<string>.Failure("Username already exists", ServiceError.Conflict);
            }

            bool isSuperUser = email == _superUserEmail;
            Student user;

            if (!isSuperUser)
            {
                user = new Student
                {
                    Username = username,
                    Email = email,
                    PasswordHash = hashedPassword,
                    Role = Enums.UserRole.Student,
                    LastSeen = DateTime.UtcNow,
                };
            } else
            {
                user = new Admin
                {
                    Username = username,
                    Email = email,
                    PasswordHash = hashedPassword,
                    Role = Enums.UserRole.Admin,
                    LastSeen = DateTime.UtcNow,
                };
            }

            // Works for both Student and Admin since Admin inherits from Student and shares the same db table
            _context.Students.Add(user);

            await _context.SaveChangesAsync();

            return _tokenService.CreateToken(user);
        }

        public async Task<ServiceResult<bool>> LogoutAsync(long userId)
        {
            _logger.LogInformation("Attempting to logout user: {id}", userId);

            var user = await _context.Students.FindAsync(userId);

            if (user == null) return ServiceResult<bool>.Failure("User not found.", ServiceError.NotFound);

            user.IsOnline = false;
            user.LastSeen = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<UserDTO>> GetCurrentUserAsync(long userId, string? role)
        {
            var user = role switch
            {
                "Teacher" => await _context.Teachers.FindAsync(userId),
                "Admin" => await _context.Admins.FindAsync(userId),
                _ => await _context.Students.FindAsync(userId),
            };

            if (user == null) return ServiceResult<UserDTO>.Failure("User not found", ServiceError.NotFound);
            
            var result = new UserDTO(
                Id: user.Id,
                Username: user.Username,
                Email: user.Email,
                CurrentXP: user.CurrentXP.ToString(),
                Role: user.Role.ToString(),
                Department: user is Teacher t ? t.Department : null,
                PhoneNumber: user is Admin a ? a.PhoneNumber : null
            );
            return ServiceResult<UserDTO>.Ok(result);
        }

        public async Task<ServiceResult<bool>> UpdateUserRoleAsync(long adminId, UpdateRoleDTO dto)
        {
            var performingAdmin = await _context.Admins.FindAsync(adminId);
            if (performingAdmin == null)
            {
                _logger.LogWarning("Unauthorized role update attempt by user {Id}", adminId);
                return ServiceResult<bool>.Failure("Unauthorized admin user.", ServiceError.Unauthorized);
            }

            UserRole newRole = dto.Role;
            long userId = dto.Id;
            var user = await _context.Students.FindAsync(userId);
            if (user == null) return ServiceResult<bool>.Failure("User not found", ServiceError.NotFound);

            if (user.Role == newRole) return ServiceResult<bool>.Ok(true);

            bool isSuperUser = performingAdmin.Email == _superUserEmail;

            // Prevent role modifications to the Super User account, even by themselves
            if (user.Email == _superUserEmail)
            {
                _logger.LogWarning("Attempted modification of Super User: {Email} by {Email}", _superUserEmail, performingAdmin.Email);
                return ServiceResult<bool>.Failure("Forbidden operation.", ServiceError.Forbidden);
            }

            // Only Super User can assign Admin role, and no one can create another Admin
            if (newRole == UserRole.Admin && !isSuperUser)
            {
                _logger.LogWarning("Admin {Email} tried to create another Admin.", performingAdmin.Email);
                return ServiceResult<bool>.Failure("Forbidden operation.", ServiceError.Forbidden);
            }

            try
            {
                // We use ExecuteSqlInterpolatedAsync to update the 'Role' (User discriminator)
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""Users""
            SET ""Role"" = {(int)newRole},
                ""Department"" = {dto.Department},
                ""PhoneNumber"" = {dto.PhoneNumber}
            WHERE ""Id"" = {userId}");

                _logger.LogInformation("Successfully updated user {Id} to {Role}", userId, newRole);
                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for user {Id}", userId);
                return ServiceResult<bool>.Failure("Internal error.", ServiceError.InternalError);
            }
        }
    }
}