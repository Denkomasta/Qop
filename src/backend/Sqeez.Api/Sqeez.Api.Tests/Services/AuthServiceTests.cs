using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services.AuthService;
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

        private AuthService CreateService(SqeezDbContext context, Mock<ITokenService>? mockTokenService = null)
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["SUPER_USER_EMAIL"]).Returns(SuperUserEmail);

            if (mockTokenService == null)
            {
                mockTokenService = new Mock<ITokenService>();
                mockTokenService.Setup(t => t.CreateToken(It.IsAny<Student>()))
                                .Returns(ServiceResult<string>.Ok("fake-jwt-token"));
            }

            var mockLogger = new Mock<ILogger<AuthService>>();

            return new AuthService(context, mockConfig.Object, mockTokenService.Object, mockLogger.Object);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndSetsOnline()
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

            Assert.Null(result.ErrorMessage);
            Assert.Equal("fake-jwt-token", result.Data);

            var dbUser = await context.Students.FindAsync(user.Id);
            Assert.True(dbUser!.IsOnline);
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

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.Unauthorized, result.ErrorCode);
        }

        [Fact]
        public async Task RegisterAsync_WithNormalEmail_CreatesStudent()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);
            var registerDto = new RegisterDTO("NewUser", "normal@sqeez.com", "pwd" );

            var result = await service.RegisterAsync(registerDto);

            Assert.Null(result.ErrorMessage);

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
            var registerDto = new RegisterDTO( "Founder", SuperUserEmail, "pwd");

            var result = await service.RegisterAsync(registerDto);

            Assert.Null(result.ErrorMessage);

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

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.Conflict, result.ErrorCode);
        }

        [Fact]
        public async Task LogoutAsync_WhenUserExists_SetsOffline()
        {
            var context = await GetInMemoryDbContext();
            var user = new Student { Username = "OnlineUser", Email = "online@sqeez.com", IsOnline = true };
            context.Students.Add(user);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.LogoutAsync(user.Id);

            Assert.Null(result.ErrorMessage);
            Assert.True(result.Data);

            var dbUser = await context.Students.FindAsync(user.Id);
            Assert.False(dbUser!.IsOnline);
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

            Assert.Null(result.ErrorMessage);
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

            Assert.NotNull(result.ErrorMessage);
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

            Assert.NotNull(result.ErrorMessage);
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
            // If we get an InternalError, it proves our logic successfully passed all the security guard clauses!
            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.InternalError, result.ErrorCode);
            Assert.Equal("Internal error.", result.ErrorMessage);
        }
    }
}