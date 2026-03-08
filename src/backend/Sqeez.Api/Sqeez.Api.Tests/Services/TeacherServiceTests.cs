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
    public class TeacherServiceTests
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

        private TeacherService CreateService(SqeezDbContext context)
        {
            var mockLogger = new Mock<ILogger<TeacherService>>();
            return new TeacherService(context, mockLogger.Object);
        }

        [Fact]
        public async Task GetTeacherByIdAsync_WhenTeacherExists_ReturnsTeacher()
        {
            var context = await GetInMemoryDbContext();
            var teacher = new Teacher { Username = "TestTeacher", Email = "test@sqeez.com", Role = UserRole.Teacher };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetTeacherByIdAsync(teacher.Id);

            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal("TestTeacher", result.Data.Username);
            Assert.Equal("test@sqeez.com", result.Data.Email);
        }

        [Fact]
        public async Task GetTeacherByIdAsync_WhenTeacherDoesNotExist_ReturnsFailure()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);

            var result = await service.GetTeacherByIdAsync(999);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
            Assert.Equal("Teacher not found.", result.ErrorMessage);
        }


        [Fact]
        public async Task CreateTeacherAsync_WhenEmailIsUnique_CreatesAndReturnsTeacher()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);
            var createDto = new CreateTeacherDto
            {
                Username = "NewTeacher",
                Email = "newteacher@sqeez.com",
                Password = "hashedpassword123",
                Department = "Math"
            };

            var result = await service.CreateTeacherAsync(createDto);

            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal("NewTeacher", result.Data.Username);

            var savedTeacher = await context.Teachers.FirstOrDefaultAsync(t => t.Email == "newteacher@sqeez.com");
            Assert.NotNull(savedTeacher);
            Assert.Equal("Math", savedTeacher.Department);
            Assert.Equal(UserRole.Teacher, savedTeacher.Role);
        }

        [Fact]
        public async Task CreateTeacherAsync_WhenEmailAlreadyExists_ReturnsConflict()
        {
            var context = await GetInMemoryDbContext();
            context.Students.Add(new Teacher { Username = "Existing", Email = "conflict@sqeez.com", Role = UserRole.Teacher });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var createDto = new CreateTeacherDto { Username = "Duplicate", Email = "conflict@sqeez.com", Password = "pwd" };

            var result = await service.CreateTeacherAsync(createDto);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.Conflict, result.ErrorCode);
            Assert.Equal("Email already in use.", result.ErrorMessage);
        }


        [Fact]
        public async Task PatchTeacherAsync_WhenTeacherExists_UpdatesProperties()
        {
            var context = await GetInMemoryDbContext();
            var teacher = new Teacher { Username = "OldName", Email = "old@sqeez.com", Role = UserRole.Teacher };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var patchDto = new PatchTeacherDto { Username = "UpdatedName", Department = "Science" };

            var result = await service.PatchTeacherAsync(teacher.Id, patchDto);

            Assert.Null(result.ErrorMessage);
            Assert.True(result.Data);

            var updatedTeacher = await context.Teachers.FindAsync(teacher.Id);
            Assert.Equal("UpdatedName", updatedTeacher!.Username);
            Assert.Equal("Science", updatedTeacher.Department);
            Assert.Equal("old@sqeez.com", updatedTeacher.Email);
        }

        [Fact]
        public async Task PatchTeacherAsync_WhenTeacherDoesNotExist_ReturnsFailure()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);
            var patchDto = new PatchTeacherDto { Username = "NewName" };

            var result = await service.PatchTeacherAsync(999, patchDto);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
            Assert.Equal("Teacher not found.", result.ErrorMessage);
        }


        [Fact]
        public async Task DeleteTeacherAsync_WhenTeacherExists_SoftDeletesTeacher()
        {
            var context = await GetInMemoryDbContext();
            var teacher = new Teacher { Username = "ToBeDeleted", Email = "delete@sqeez.com", Role = UserRole.Teacher };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteTeacherAsync(teacher.Id);

            Assert.Null(result.ErrorMessage);

            var deletedTeacher = await context.Teachers.FindAsync(teacher.Id);
            Assert.NotNull(deletedTeacher!.ArchivedAt);
        }

        [Fact]
        public async Task DeleteTeacherAsync_WhenTeacherDoesNotExist_ReturnsFailure()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);

            var result = await service.DeleteTeacherAsync(999);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
        }

        [Fact]
        public async Task GetAllTeachersAsync_WhenStrictRoleIsTrue_ExcludesAdmins()
        {
            var context = await GetInMemoryDbContext();
            context.Teachers.Add(new Teacher { Username = "NormalTeacher", Role = UserRole.Teacher });
            context.Admins.Add(new Admin { Username = "TeachingAdmin", Role = UserRole.Admin });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new TeacherFilterDto { StrictRoleOnly = true, PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllTeachersAsync(filter);

            Assert.Equal(1, result.Data!.TotalCount);
            Assert.Equal("NormalTeacher", result.Data.Data.First().Username);
        }

        [Fact]
        public async Task GetAllTeachersAsync_WhenStrictRoleIsFalse_IncludesAdmins()
        {
            var context = await GetInMemoryDbContext();
            context.Teachers.Add(new Teacher { Username = "NormalTeacher", Role = UserRole.Teacher });
            context.Admins.Add(new Admin { Username = "TeachingAdmin", Role = UserRole.Admin });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new TeacherFilterDto { StrictRoleOnly = false, PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllTeachersAsync(filter);

            Assert.Equal(2, result.Data!.TotalCount);
        }

        [Fact]
        public async Task GetAllTeachersAsync_WithSearchTerm_ReturnsFilteredResults()
        {
            var context = await GetInMemoryDbContext();
            context.Teachers.AddRange(
                new Teacher { Username = "JohnDoe", Email = "john@sqeez.com", Role = UserRole.Teacher },
                new Teacher { Username = "JaneSmith", Email = "jane@sqeez.com", Role = UserRole.Teacher }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new TeacherFilterDto { SearchTerm = "smith", PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllTeachersAsync(filter);

            Assert.Single(result.Data!.Data);
            Assert.Equal("JaneSmith", result.Data.Data.First().Username);
        }
    }
}