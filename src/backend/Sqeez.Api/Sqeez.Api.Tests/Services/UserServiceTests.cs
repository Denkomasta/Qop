using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services.Interfaces;
using Sqeez.Api.Services.UserService;

namespace Sqeez.Api.Tests.Services
{
    public class UserServiceTests
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

        private UserService CreateService(SqeezDbContext context)
        {
            var mockLogger = new Mock<ILogger<UserService>>();
            var mockedFileService = new Mock<IFileStorageService>();
            return new UserService(context, mockLogger.Object, mockedFileService.Object);
        }

        // ==========================================
        // 1. GET BY ID TESTS
        // ==========================================

        [Fact]
        public async Task GetUserByIdAsync_WhenUserExists_ReturnsCorrectPolymorphicDto()
        {
            var context = await GetInMemoryDbContext();
            var admin = new Admin { Username = "TestAdmin", Email = "admin@sqeez.com", Role = UserRole.Admin, PhoneNumber = "123456789" };
            context.Students.Add(admin);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetUserByIdAsync(admin.Id);

            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);

            var adminDto = Assert.IsType<AdminDto>(result.Data);
            Assert.Equal("TestAdmin", adminDto.Username);
            Assert.Equal("123456789", adminDto.PhoneNumber);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenUserDoesNotExist_ReturnsFailure()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);

            var result = await service.GetUserByIdAsync(999);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
        }

        // ==========================================
        // 2. CREATE TESTS
        // ==========================================

        [Fact]
        public async Task CreateUserAsync_WithAdminDto_CreatesAdminInDatabase()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);

            var createDto = new CreateAdminDto
            {
                Username = "NewAdmin",
                Email = "new@sqeez.com",
                Password = "pwd",
                Department = "IT",
                PhoneNumber = "+1234567890"
            };

            var result = await service.CreateUserAsync(createDto);

            Assert.Null(result.ErrorMessage);
            var resultDto = Assert.IsType<AdminDto>(result.Data);
            Assert.Equal("IT", resultDto.Department);

            var savedUser = await context.Students.FirstOrDefaultAsync(a => a.Email == "new@sqeez.com");
            var savedAdmin = Assert.IsType<Admin>(savedUser);
            Assert.Equal("+1234567890", savedAdmin.PhoneNumber);
            Assert.Equal(UserRole.Admin, savedAdmin.Role);
        }

        [Fact]
        public async Task CreateUserAsync_WhenEmailAlreadyExists_ReturnsConflict()
        {
            var context = await GetInMemoryDbContext();
            context.Students.Add(new Student { Username = "Existing", Email = "conflict@sqeez.com", Role = UserRole.Student });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var createDto = new CreateTeacherDto { Username = "Duplicate", Email = "conflict@sqeez.com", Password = "pwd" };

            var result = await service.CreateUserAsync(createDto);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.Conflict, result.ErrorCode);
        }

        // ==========================================
        // 3. PATCH TESTS
        // ==========================================

        [Fact]
        public async Task PatchUserAsync_WhenAdminExists_UpdatesBaseAndDerivedProperties()
        {
            var context = await GetInMemoryDbContext();
            var admin = new Admin { Username = "OldName", Email = "old@sqeez.com", Role = UserRole.Admin, Department = "OldDept" };
            context.Students.Add(admin);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var patchDto = new PatchAdminDto { Username = "UpdatedName", Department = "HR", PhoneNumber = "999-999-9999" };

            var result = await service.PatchUserAsync(admin.Id, patchDto);

            Assert.Null(result.ErrorMessage);
            var updatedAdmin = Assert.IsType<AdminDto>(result.Data);

            Assert.Equal("UpdatedName", updatedAdmin.Username);
            Assert.Equal("HR", updatedAdmin.Department);
            Assert.Equal("999-999-9999", updatedAdmin.PhoneNumber);
        }

        // ==========================================
        // 4. ARCHIVE TESTS
        // ==========================================

        [Fact]
        public async Task ArchiveUserAsync_WhenUserExists_SoftDeletesUser()
        {
            var context = await GetInMemoryDbContext();
            var student = new Student { Username = "ToBeDeleted", Email = "delete@sqeez.com", Role = UserRole.Student };
            context.Students.Add(student);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.ArchiveUserAsync(student.Id);

            Assert.Null(result.ErrorMessage);
            var deletedStudent = await context.Students.FindAsync(student.Id);
            Assert.NotNull(deletedStudent!.ArchivedAt);
        }

        // ==========================================
        // 5. GET ALL TESTS (FILTERS & PAGINATION)
        // ==========================================

        [Fact]
        public async Task GetAllUsersAsync_WhenRoleIsTeacherAndStrictIsFalse_IncludesTeachersAndAdmins()
        {
            var context = await GetInMemoryDbContext();
            context.Students.AddRange(
                new Student { Username = "JustStudent", Role = UserRole.Student },
                new Teacher { Username = "MathTeacher", Role = UserRole.Teacher },
                new Admin { Username = "SystemAdmin", Role = UserRole.Admin }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new UserFilterDto { Role = UserRole.Teacher, StrictRoleOnly = false, PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllUsersAsync(filter);

            Assert.Equal(2, result.Data!.TotalCount);
            Assert.Contains(result.Data.Data, u => u.Username == "MathTeacher");
            Assert.Contains(result.Data.Data, u => u.Username == "SystemAdmin");
            Assert.DoesNotContain(result.Data.Data, u => u.Username == "JustStudent");
        }

        [Fact]
        public async Task GetAllUsersAsync_WhenRoleIsTeacherAndStrictIsTrue_IncludesOnlyTeachers()
        {
            var context = await GetInMemoryDbContext();
            context.Students.AddRange(
                new Student { Username = "JustStudent", Role = UserRole.Student },
                new Teacher { Username = "MathTeacher", Role = UserRole.Teacher },
                new Admin { Username = "SystemAdmin", Role = UserRole.Admin }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new UserFilterDto { Role = UserRole.Teacher, StrictRoleOnly = true, PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllUsersAsync(filter);

            Assert.Equal(1, result.Data!.TotalCount);
            Assert.Equal("MathTeacher", result.Data.Data.First().Username);
        }

        [Fact]
        public async Task GetAllUsersAsync_WithSearchTerm_ReturnsFilteredResults()
        {
            var context = await GetInMemoryDbContext();
            context.Students.AddRange(
                new Student { Username = "JohnDoe", Email = "john@sqeez.com", Role = UserRole.Student },
                new Teacher { Username = "JaneSmith", Email = "jane@sqeez.com", Role = UserRole.Teacher }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new UserFilterDto { SearchTerm = "smith", PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllUsersAsync(filter);

            Assert.Single(result.Data!.Data);
            Assert.Equal("JaneSmith", result.Data.Data.First().Username);
        }

        [Fact]
        public async Task GetAllUsersAsync_WithPagination_ReturnsCorrectPageAndCount()
        {
            var context = await GetInMemoryDbContext();
            // Create 5 students named A through E
            for (int i = 0; i < 5; i++)
            {
                context.Students.Add(new Student
                {
                    Username = $"Student{(char)('A' + i)}",
                    Role = UserRole.Student
                });
            }
            await context.SaveChangesAsync();

            var service = CreateService(context);
            // Get page 2, size 2 (should return StudentC and StudentD)
            var filter = new UserFilterDto { PageNumber = 2, PageSize = 2 };

            var result = await service.GetAllUsersAsync(filter);

            Assert.Equal(5, result.Data!.TotalCount);
            Assert.Equal(2, result.Data.Data.Count());
            Assert.Equal("StudentC", result.Data.Data.First().Username);
        }

        [Fact]
        public async Task GetAllUsersAsync_WithIsOnline_FiltersByLastSeen()
        {
            var context = await GetInMemoryDbContext();
            context.Students.AddRange(
                new Student { Username = "OnlineUser", Role = UserRole.Student, LastSeen = DateTime.UtcNow.AddMinutes(-5) },
                new Student { Username = "OfflineUser", Role = UserRole.Student, LastSeen = DateTime.UtcNow.AddMinutes(-30) }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new UserFilterDto { IsOnline = true, PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllUsersAsync(filter);

            Assert.Single(result.Data!.Data);
            Assert.Equal("OnlineUser", result.Data.Data.First().Username);
        }

        [Fact]
        public async Task GetAllUsersAsync_WithIsArchived_FiltersCorrectly()
        {
            var context = await GetInMemoryDbContext();
            context.Students.AddRange(
                new Student { Username = "ActiveUser", Role = UserRole.Student, ArchivedAt = null },
                new Student { Username = "ArchivedUser", Role = UserRole.Student, ArchivedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // 1. Test IsArchived = true
            var archivedFilter = new UserFilterDto { IsArchived = true, PageNumber = 1, PageSize = 10 };
            var archivedResult = await service.GetAllUsersAsync(archivedFilter);
            Assert.Single(archivedResult.Data!.Data);
            Assert.Equal("ArchivedUser", archivedResult.Data.Data.First().Username);

            // 2. Test IsArchived = false
            var activeFilter = new UserFilterDto { IsArchived = false, PageNumber = 1, PageSize = 10 };
            var activeResult = await service.GetAllUsersAsync(activeFilter);
            Assert.Single(activeResult.Data!.Data);
            Assert.Equal("ActiveUser", activeResult.Data.Data.First().Username);
        }

        [Fact]
        public async Task GetAllUsersAsync_WithDepartment_OnlyReturnsMatchingTeachersAndAdmins()
        {
            var context = await GetInMemoryDbContext();
            context.Students.AddRange(
                new Student { Username = "NormalStudent", Role = UserRole.Student },
                new Teacher { Username = "MathTeacher", Role = UserRole.Teacher, Department = "Math" },
                new Teacher { Username = "ScienceTeacher", Role = UserRole.Teacher, Department = "Science" },
                new Admin { Username = "MathAdmin", Role = UserRole.Admin, Department = "Math" }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new UserFilterDto { Department = "Math", PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllUsersAsync(filter);

            Assert.Equal(2, result.Data!.TotalCount);
            Assert.Contains(result.Data.Data, u => u.Username == "MathTeacher");
            Assert.Contains(result.Data.Data, u => u.Username == "MathAdmin");
            Assert.DoesNotContain(result.Data.Data, u => u.Username == "ScienceTeacher");
        }

        [Fact]
        public async Task GetAllUsersAsync_WithSchoolClassId_FiltersCorrectly()
        {
            var context = await GetInMemoryDbContext();
            context.Students.AddRange(
                new Student { Username = "Class1Student", Role = UserRole.Student, SchoolClassId = 1 },
                new Teacher { Username = "Class1Teacher", Role = UserRole.Teacher, SchoolClassId = 1 },
                new Student { Username = "Class2Student", Role = UserRole.Student, SchoolClassId = 2 }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new UserFilterDto { SchoolClassId = 1, PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllUsersAsync(filter);

            Assert.Equal(2, result.Data!.TotalCount);
            Assert.DoesNotContain(result.Data.Data, u => u.Username == "Class2Student");
        }
    }
}