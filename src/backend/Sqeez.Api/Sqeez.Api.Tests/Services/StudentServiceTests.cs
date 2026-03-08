using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services.UserService;
using Xunit;

namespace Sqeez.Api.Tests.Services
{
    public class StudentServiceTests
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

        private StudentService CreateService(SqeezDbContext context)
        {
            var mockLogger = new Mock<ILogger<StudentService>>();
            return new StudentService(context, mockLogger.Object);
        }


        [Fact]
        public async Task GetStudentByIdAsync_WhenStudentExists_ReturnsStudent()
        {
            var context = await GetInMemoryDbContext();
            var student = new Student { Username = "TestStudent", Email = "test@sqeez.com", Role = UserRole.Student };
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetStudentByIdAsync(student.Id);

            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal("TestStudent", result.Data.Username);
            Assert.Equal("test@sqeez.com", result.Data.Email);
        }

        [Fact]
        public async Task GetStudentByIdAsync_WhenStudentDoesNotExist_ReturnsFailure()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);

            var result = await service.GetStudentByIdAsync(999);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
            Assert.Equal("Student not found.", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateStudentAsync_WhenEmailIsUnique_CreatesAndReturnsStudent()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);
            var createDto = new CreateStudentDto
            {
                Username = "NewStudent",
                Email = "newstudent@sqeez.com",
                Password = "hashedpassword123",
                SchoolClassId = 1
            };

            var result = await service.CreateStudentAsync(createDto);

            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal("NewStudent", result.Data.Username);

            var savedStudent = await context.Students.FirstOrDefaultAsync(s => s.Email == "newstudent@sqeez.com");
            Assert.NotNull(savedStudent);
            Assert.Equal(1, savedStudent.SchoolClassId);
            Assert.Equal(UserRole.Student, savedStudent.Role);
        }

        [Fact]
        public async Task CreateStudentAsync_WhenEmailAlreadyExists_ReturnsConflict()
        {
            var context = await GetInMemoryDbContext();
            context.Students.Add(new Student { Username = "Existing", Email = "conflict@sqeez.com", Role = UserRole.Student });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var createDto = new CreateStudentDto { Username = "Duplicate", Email = "conflict@sqeez.com", Password = "pwd" };

            var result = await service.CreateStudentAsync(createDto);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.Conflict, result.ErrorCode);
            Assert.Equal("Email already in use.", result.ErrorMessage);
        }

        [Fact]
        public async Task PatchStudentAsync_WhenStudentExists_UpdatesProperties()
        {
            var context = await GetInMemoryDbContext();
            var student = new Student { Username = "OldName", Email = "old@sqeez.com", Role = UserRole.Student, SchoolClassId = 1 };
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var patchDto = new PatchStudentDto { Username = "UpdatedName", SchoolClassId = 2 };

            var result = await service.PatchStudentAsync(student.Id, patchDto);

            Assert.Null(result.ErrorMessage);
            Assert.True(result.Data);

            var updatedStudent = await context.Students.FindAsync(student.Id);
            Assert.Equal("UpdatedName", updatedStudent!.Username);
            Assert.Equal(2, updatedStudent.SchoolClassId);
            Assert.Equal("old@sqeez.com", updatedStudent.Email);
        }

        [Fact]
        public async Task PatchStudentAsync_WhenStudentDoesNotExist_ReturnsFailure()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);
            var patchDto = new PatchStudentDto { Username = "NewName" };

            var result = await service.PatchStudentAsync(999, patchDto);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
            Assert.Equal("Student not found.", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteStudentAsync_WhenStudentExists_SoftDeletesStudent()
        {
            var context = await GetInMemoryDbContext();
            var student = new Student { Username = "ToBeDeleted", Email = "delete@sqeez.com", Role = UserRole.Student };
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteStudentAsync(student.Id);

            Assert.Null(result.ErrorMessage);

            var deletedStudent = await context.Students.FindAsync(student.Id);
            Assert.NotNull(deletedStudent!.ArchivedAt);
        }

        [Fact]
        public async Task DeleteStudentAsync_WhenStudentDoesNotExist_ReturnsFailure()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);

            var result = await service.DeleteStudentAsync(999);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
        }

        [Fact]
        public async Task GetAllStudentsAsync_WhenStrictRoleIsTrue_ExcludesTeachersAndAdmins()
        {
            var context = await GetInMemoryDbContext();
            context.Students.Add(new Student { Username = "NormalStudent", Role = UserRole.Student });
            context.Teachers.Add(new Teacher { Username = "TeacherUser", Role = UserRole.Teacher });
            context.Admins.Add(new Admin { Username = "AdminUser", Role = UserRole.Admin });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new StudentFilterDto { StrictRoleOnly = true, PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllStudentsAsync(filter);

            Assert.Equal(1, result.Data!.TotalCount);
            Assert.Equal("NormalStudent", result.Data.Data.First().Username);
        }

        [Fact]
        public async Task GetAllStudentsAsync_WhenStrictRoleIsFalse_IncludesEveryone()
        {
            var context = await GetInMemoryDbContext();
            context.Students.Add(new Student { Username = "NormalStudent", Role = UserRole.Student });
            context.Teachers.Add(new Teacher { Username = "TeacherUser", Role = UserRole.Teacher });
            context.Admins.Add(new Admin { Username = "AdminUser", Role = UserRole.Admin });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new StudentFilterDto { StrictRoleOnly = false, PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllStudentsAsync(filter);

            Assert.Equal(3, result.Data!.TotalCount);
        }

        [Fact]
        public async Task GetAllStudentsAsync_WithPagination_ReturnsCorrectPage()
        {
            var context = await GetInMemoryDbContext();
            context.Students.AddRange(
                new Student { Username = "StudentA", Role = UserRole.Student },
                new Student { Username = "StudentB", Role = UserRole.Student },
                new Student { Username = "StudentC", Role = UserRole.Student }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new StudentFilterDto { StrictRoleOnly = true, PageNumber = 2, PageSize = 2 };

            var result = await service.GetAllStudentsAsync(filter);

            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.TotalCount);
            Assert.Single(result.Data.Data);
        }

        [Fact]
        public async Task GetAllStudentsAsync_WithSearchTerm_ReturnsFilteredResults()
        {
            var context = await GetInMemoryDbContext();
            context.Students.AddRange(
                new Student { Username = "JohnDoe", Email = "john@sqeez.com", Role = UserRole.Student },
                new Student { Username = "JaneSmith", Email = "jane@sqeez.com", Role = UserRole.Student }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new StudentFilterDto { SearchTerm = "smith", PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllStudentsAsync(filter);

            Assert.Single(result.Data!.Data);
            Assert.Equal("JaneSmith", result.Data.Data.First().Username);
        }
    }
}