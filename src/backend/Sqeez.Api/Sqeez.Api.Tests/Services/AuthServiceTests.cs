using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services.AuthService;
using Sqeez.Api.Services.Interfaces;
using Sqeez.Api.Services.TokenService;
using BC = BCrypt.Net.BCrypt;

namespace Sqeez.Api.Tests.Services
{
    public class AuthServiceTests
    {
        private const string SuperUserEmail = "founder@sqeez.com";

        private async Task<SqeezDbContext> GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SqeezDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SqeezDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return context;
        }

        private AuthService CreateService(SqeezDbContext context, Mock<ITokenService>? mockTokenService = null, Mock<ISystemConfigService>? mockConfigService = null)
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["SUPER_USER_EMAIL"]).Returns(SuperUserEmail);

            if (mockTokenService == null)
            {
                mockTokenService = new Mock<ITokenService>();
                mockTokenService.Setup(t => t.CreateToken(It.IsAny<Student>()))
                                .Returns(ServiceResult<string>.Ok("fake-jwt-token"));
                mockTokenService.Setup(t => t.GenerateRefreshToken())
                                .Returns("fake-refresh-token");
            }

            if (mockConfigService == null)
            {
                mockConfigService = new Mock<ISystemConfigService>();
                // Fake the global config so Registration is open and Max Sessions = 3
                mockConfigService.Setup(c => c.GetConfigAsync())
                    .ReturnsAsync(ServiceResult<SystemConfigDto>.Ok(
                        new SystemConfigDto("Sqeez", "", "", "en", "24/25", true, true, 10, 3)
                    ));
            }

            var mockLogger = new Mock<ILogger<AuthService>>();

            return new AuthService(context, mockConfig.Object, mockTokenService.Object, mockConfigService.Object, mockLogger.Object);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsTokensAndSetsOnline()
        {
            var context = await GetInMemoryDbContext();
            string password = "MySecretPassword123!";
            var user = new Student
            {
                Username = "LoginUser",
                Email = "login@sqeez.com",
                PasswordHash = BC.HashPassword(password),
                IsOnline = false
            };
            context.Students.Add(user);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var loginDto = new LoginDTO("login@sqeez.com", password);

            var result = await service.LoginAsync(loginDto);

            Assert.True(result.Success);
            Assert.Equal("fake-jwt-token", result.Data!.AccessToken);
            Assert.Equal("fake-refresh-token", result.Data.RefreshToken);

            var dbUser = await context.Students.FindAsync(user.Id);
            Assert.True(dbUser!.IsOnline);

            // Verify a session was saved
            var sessionCount = await context.UserSessions.CountAsync(s => s.UserId == user.Id);
            Assert.Equal(1, sessionCount);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ReturnsUnauthorized()
        {
            var context = await GetInMemoryDbContext();
            var user = new Student { Email = "login@sqeez.com", PasswordHash = BC.HashPassword("CorrectPassword") };
            context.Students.Add(user);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var loginDto = new LoginDTO("login@sqeez.com", "WrongPassword");

            var result = await service.LoginAsync(loginDto);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.Unauthorized, result.ErrorCode);
        }

        [Fact]
        public async Task RegisterAsync_WithNormalEmail_CreatesStudent()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);
            var registerDto = new RegisterDTO("NewUser", "normal@sqeez.com", "pwd");

            var result = await service.RegisterAsync(registerDto);

            Assert.True(result.Success);
            Assert.NotNull(result.Data!.AccessToken);

            var savedUser = await context.Students.FirstOrDefaultAsync(u => u.Email == "normal@sqeez.com");
            Assert.NotNull(savedUser);
            Assert.Equal(UserRole.Student, savedUser.Role);
            Assert.True(BC.Verify("pwd", savedUser.PasswordHash));
        }

        [Fact]
        public async Task RegisterAsync_WithSuperUserEmail_CreatesAdmin()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);
            var registerDto = new RegisterDTO("Founder", SuperUserEmail, "pwd");

            var result = await service.RegisterAsync(registerDto);

            Assert.True(result.Success);

            var savedUser = await context.Admins.FirstOrDefaultAsync(u => u.Email == SuperUserEmail);
            Assert.NotNull(savedUser);
            Assert.Equal(UserRole.Admin, savedUser.Role);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ReturnsConflict()
        {
            var context = await GetInMemoryDbContext();
            context.Students.Add(new Student { Username = "FirstUser", Email = "taken@sqeez.com" });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var registerDto = new RegisterDTO("SecondUser", "taken@sqeez.com", "pwd");

            var result = await service.RegisterAsync(registerDto);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.Conflict, result.ErrorCode);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenValid_ReturnsNewTokensAndRevokesOld()
        {
            var context = await GetInMemoryDbContext();
            var user = new Student { Username = "RefreshUser", Email = "refresh@sqeez.com" };
            context.Students.Add(user);
            await context.SaveChangesAsync();

            var oldSession = new UserSession
            {
                UserId = user.Id,
                RefreshToken = "old-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsRevoked = false
            };
            context.UserSessions.Add(oldSession);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.RefreshTokenAsync(new RefreshTokenDto("old-refresh-token"));

            Assert.True(result.Success);
            Assert.Equal("fake-jwt-token", result.Data!.AccessToken);
            Assert.Equal("fake-refresh-token", result.Data.RefreshToken);

            // Assert that the old session was properly revoked
            var dbOldSession = await context.UserSessions.FindAsync(oldSession.Id);
            Assert.True(dbOldSession!.IsRevoked);
        }

        [Fact]
        public async Task LogoutAsync_WhenUserExists_SetsOfflineAndRevokesSessions()
        {
            var context = await GetInMemoryDbContext();
            var user = new Student { Username = "OnlineUser", Email = "online@sqeez.com", IsOnline = true };
            context.Students.Add(user);

            var session = new UserSession { UserId = user.Id, RefreshToken = "my-token", IsRevoked = false };
            context.UserSessions.Add(session);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.LogoutAsync(user.Id, "my-token");

            Assert.True(result.Success);
            Assert.True(result.Data);

            var dbUser = await context.Students.FindAsync(user.Id);
            Assert.False(dbUser!.IsOnline);

            var dbSession = await context.UserSessions.FindAsync(session.Id);
            Assert.True(dbSession!.IsRevoked);
        }

        [Fact]
        public async Task GetCurrentUserAsync_WhenUserIsTeacher_ReturnsTeacherProperties()
        {
            var context = await GetInMemoryDbContext();
            var teacher = new Teacher { Username = "MrSmith", Email = "smith@sqeez.com", Role = UserRole.Teacher, Department = "History" };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetCurrentUserAsync(teacher.Id, "Teacher");

            Assert.True(result.Success);
            Assert.Equal("History", result.Data!.Department);
            Assert.Null(result.Data.PhoneNumber);
        }

        [Fact]
        public async Task UpdateUserRoleAsync_WhenModifyingSuperUser_ReturnsForbidden()
        {
            var context = await GetInMemoryDbContext();

            var performingAdmin = new Admin { Username = "Admin2", Email = "admin2@sqeez.com" };
            var superUser = new Admin { Username = "Founder", Email = SuperUserEmail, Role = UserRole.Admin };

            context.Admins.AddRange(performingAdmin, superUser);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new UpdateRoleDTO(superUser.Id, UserRole.Teacher);

            var result = await service.UpdateUserRoleAsync(performingAdmin.Id, dto);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.Forbidden, result.ErrorCode);
        }

        [Fact]
        public async Task UpdateUserRoleAsync_WhenNormalAdminCreatesAdmin_ReturnsForbidden()
        {
            var context = await GetInMemoryDbContext();
            var performingAdmin = new Admin { Username = "Admin2", Email = "admin2@sqeez.com" };
            var targetStudent = new Student { Username = "Student", Email = "student@sqeez.com", Role = UserRole.Student };

            context.Students.AddRange(performingAdmin, targetStudent);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new UpdateRoleDTO(targetStudent.Id, UserRole.Admin);

            var result = await service.UpdateUserRoleAsync(performingAdmin.Id, dto);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.Forbidden, result.ErrorCode);
        }

        [Fact]
        public async Task UpdateUserRoleAsync_WhenValidRequest_ReachesSqlExecutionAndCatchesInMemoryError()
        {
            var context = await GetInMemoryDbContext();

            var founder = new Admin { Username = "Founder", Email = SuperUserEmail };
            var targetStudent = new Student { Username = "Student", Email = "student@sqeez.com", Role = UserRole.Student };

            context.Students.AddRange(founder, targetStudent);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new UpdateRoleDTO(targetStudent.Id, UserRole.Admin);

            var result = await service.UpdateUserRoleAsync(founder.Id, dto);

            // Because EF Core InMemory DOES NOT support raw SQL (ExecuteSqlInterpolatedAsync), 
            // it throws an InvalidOperationException, which your catch block handles.
            Assert.False(result.Success);
            Assert.Equal(ServiceError.InternalError, result.ErrorCode);
            Assert.Equal("Internal error.", result.ErrorMessage);
        }
    }
}