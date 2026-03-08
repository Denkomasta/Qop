using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services;
using Xunit;

namespace Sqeez.Api.Tests.Services
{
    public class AdminServiceTests
    {
        private async Task<SqeezDbContext> GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SqeezDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SqeezDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return context;
        }

        private AdminService CreateService(SqeezDbContext context)
        {
            var mockLogger = new Mock<ILogger<AdminService>>();
            return new AdminService(context, mockLogger.Object);
        }

        [Fact]
        public async Task GetAdminByIdAsync_WhenAdminExists_ReturnsAdmin()
        {
            var context = await GetInMemoryDbContext();
            var admin = new Admin { Username = "TestAdmin", Email = "test@sqeez.com", Role = UserRole.Admin };
            context.Admins.Add(admin);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetAdminByIdAsync(admin.Id);

            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal("TestAdmin", result.Data.Username);
            Assert.Equal("test@sqeez.com", result.Data.Email);
        }

        [Fact]
        public async Task GetAdminByIdAsync_WhenAdminDoesNotExist_ReturnsFailure()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);

            var result = await service.GetAdminByIdAsync(999);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
            Assert.Equal("Admin not found.", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateAdminAsync_WhenEmailIsUnique_CreatesAndReturnsAdmin()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);
            var createDto = new CreateAdminDto
            {
                Username = "NewAdmin",
                Email = "new@sqeez.com",
                Password = "hashedpassword123",
                Department = "IT"
            };

            var result = await service.CreateAdminAsync(createDto);

            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal("NewAdmin", result.Data.Username);

            var savedAdmin = await context.Admins.FirstOrDefaultAsync(a => a.Email == "new@sqeez.com");
            Assert.NotNull(savedAdmin);
            Assert.Equal("IT", savedAdmin.Department);
            Assert.Equal(UserRole.Admin, savedAdmin.Role);
        }

        [Fact]
        public async Task CreateAdminAsync_WhenEmailAlreadyExists_ReturnsConflict()
        {
            var context = await GetInMemoryDbContext();
            context.Students.Add(new Admin { Username = "Existing", Email = "conflict@sqeez.com", Role = UserRole.Admin });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var createDto = new CreateAdminDto { Username = "Duplicate", Email = "conflict@sqeez.com", Password = "pwd" };

            var result = await service.CreateAdminAsync(createDto);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal("Email already in use.", result.ErrorMessage);
        }

        [Fact]
        public async Task PatchAdminAsync_WhenAdminExists_UpdatesProperties()
        {
            var context = await GetInMemoryDbContext();
            var admin = new Admin { Username = "OldName", Email = "old@sqeez.com", Role = UserRole.Admin };
            context.Admins.Add(admin);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var patchDto = new PatchAdminDto { Username = "UpdatedName", Department = "HR" };

            var result = await service.PatchAdminAsync(admin.Id, patchDto);

            Assert.Null(result.ErrorMessage);
            Assert.True(result.Data);

            var updatedAdmin = await context.Admins.FindAsync(admin.Id);
            Assert.Equal("UpdatedName", updatedAdmin!.Username);
            Assert.Equal("HR", updatedAdmin.Department);
            Assert.Equal("old@sqeez.com", updatedAdmin.Email);
        }

        [Fact]
        public async Task PatchAdminAsync_WhenAdminDoesNotExist_ReturnsFailure()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);
            var patchDto = new PatchAdminDto { Username = "NewName" };

            var result = await service.PatchAdminAsync(999, patchDto);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
            Assert.Equal("Admin not found.", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteAdminAsync_WhenAdminExists_SoftDeletesAdmin()
        {
            var context = await GetInMemoryDbContext();
            var admin = new Admin { Username = "ToBeDeleted", Email = "delete@sqeez.com", Role = UserRole.Admin };
            context.Admins.Add(admin);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteAdminAsync(admin.Id);

            Assert.Null(result.ErrorMessage);

            var deletedAdmin = await context.Admins.FindAsync(admin.Id);
            Assert.NotNull(deletedAdmin!.ArchivedAt);
        }

        [Fact]
        public async Task DeleteAdminAsync_WhenAdminDoesNotExist_SoftDeletesAdmin()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);

            var result = await service.DeleteAdminAsync(999);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
        }

        [Fact]
        public async Task GetAllAdminsAsync_WithPagination_ReturnsCorrectPage()
        {
            var context = await GetInMemoryDbContext();
            context.Admins.AddRange(
                new Admin { Username = "AdminA", Email = "a@sqeez.com", Role = UserRole.Admin },
                new Admin { Username = "AdminB", Email = "b@sqeez.com", Role = UserRole.Admin },
                new Admin { Username = "AdminC", Email = "c@sqeez.com", Role = UserRole.Admin }
            );
            context.Teachers.Add(new Teacher { Username = "Teacher", Email = "t@sqeez.com", Role = UserRole.Teacher });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new AdminFilterDto { PageNumber = 2, PageSize = 2 };

            var result = await service.GetAllAdminsAsync(filter);

            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.TotalCount);
            Assert.Single(result.Data.Data);
        }

        [Fact]
        public async Task GetAllAdminsAsync_WithSearchTerm_ReturnsFilteredResults()
        {
            var context = await GetInMemoryDbContext();
            context.Admins.AddRange(
                new Admin { Username = "JohnDoe", Email = "john@sqeez.com", Role = UserRole.Admin },
                new Admin { Username = "JaneSmith", Email = "jane@sqeez.com", Role = UserRole.Admin }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new AdminFilterDto { SearchTerm = "smith", PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllAdminsAsync(filter);

            Assert.Single(result.Data!.Data);
            Assert.Equal("JaneSmith", result.Data.Data.First().Username);
        }
    }
}