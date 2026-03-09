using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Academics;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services.SubjectService;
using Xunit;

namespace Sqeez.Api.Tests.Services
{
    public class SubjectServiceTests
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

        private SubjectService CreateService(SqeezDbContext context)
        {
            var mockLogger = new Mock<ILogger<SubjectService>>();
            return new SubjectService(context, mockLogger.Object);
        }

        [Fact]
        public async Task GetSubjectByIdAsync_WhenSubjectExists_ReturnsSubject()
        {
            var context = await GetInMemoryDbContext();
            var subject = new Subject { Name = "Mathematics", Code = "MATH101", StartDate = DateTime.UtcNow };
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetSubjectByIdAsync(subject.Id);

            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal("Mathematics", result.Data.Name);
            Assert.Equal("MATH101", result.Data.Code);
        }

        [Fact]
        public async Task GetSubjectByIdAsync_WhenSubjectDoesNotExist_ReturnsFailure()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);

            var result = await service.GetSubjectByIdAsync(999);

            Assert.NotNull(result.ErrorMessage);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
            Assert.Equal("Subject not found.", result.ErrorMessage);
        }

        [Fact]
        public async Task CreateSubjectAsync_CreatesAndReturnsSubject()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);
            var createDto = new CreateSubjectDto
            (
                "Physics",
                "PHYS201",
                "Intro to Physics",
                DateTime.UtcNow
            );

            var result = await service.CreateSubjectAsync(createDto);

            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal("Physics", result.Data.Name);

            var savedSubject = await context.Subjects.FirstOrDefaultAsync(s => s.Code == "PHYS201");
            Assert.NotNull(savedSubject);
            Assert.Equal("Intro to Physics", savedSubject.Description);
        }

        [Fact]
        public async Task PatchSubjectAsync_WhenSubjectExists_UpdatesPropertiesAndValidatesTeacher()
        {
            var context = await GetInMemoryDbContext();

            // Seed a Teacher so the validation passes
            var teacher = new Teacher { Username = "Mr. Smith", Email = "smith@sqeez.com", Role = UserRole.Teacher };
            context.Teachers.Add(teacher);

            var subject = new Subject { Name = "Old Name", Code = "OLD1", StartDate = DateTime.UtcNow };
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var patchDto = new PatchSubjectDto
            (
                "New Name",
                TeacherId: teacher.Id
            );

            var result = await service.PatchSubjectAsync(subject.Id, patchDto);

            Assert.Null(result.ErrorMessage);

            var updatedSubject = await context.Subjects.FindAsync(subject.Id);
            Assert.Equal("New Name", updatedSubject!.Name);
            Assert.Equal("OLD1", updatedSubject.Code);
            Assert.Equal(teacher.Id, updatedSubject.TeacherId);
        }

        [Fact]
        public async Task DeleteSubjectAsync_WhenSubjectIsEmpty_HardDeletesSubject()
        {
            var context = await GetInMemoryDbContext();
            var subject = new Subject { Name = "Empty Class", StartDate = DateTime.UtcNow };
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteSubjectAsync(subject.Id);

            Assert.Null(result.ErrorMessage);

            // Verify it was actually removed from the DB
            var deletedSubject = await context.Subjects.FindAsync(subject.Id);
            Assert.Null(deletedSubject);
        }

        [Fact]
        public async Task DeleteSubjectAsync_WhenSubjectHasEnrollments_SoftDeletesSubject()
        {
            var context = await GetInMemoryDbContext();
            var subject = new Subject { Name = "Populated Class", StartDate = DateTime.UtcNow };
            context.Subjects.Add(subject);

            // Add an enrollment to trigger the soft delete
            var student = new Student { Username = "TestStudent", Email = "student@sqeez.com" };
            context.Enrollments.Add(new Enrollment { Student = student, Subject = subject, EnrolledAt = DateTime.UtcNow });

            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteSubjectAsync(subject.Id);

            Assert.Null(result.ErrorMessage);

            // Verify it was NOT removed, but EndDate was set
            var softDeletedSubject = await context.Subjects.FindAsync(subject.Id);
            Assert.NotNull(softDeletedSubject);
            Assert.NotNull(softDeletedSubject.EndDate);
            Assert.True(softDeletedSubject.EndDate <= DateTime.UtcNow);
        }

        [Fact]
        public async Task GetAllSubjectsAsync_WithSearchTerm_ReturnsFilteredResults()
        {
            var context = await GetInMemoryDbContext();
            context.Subjects.AddRange(
                new Subject { Name = "Biology", Code = "BIO1", StartDate = DateTime.UtcNow },
                new Subject { Name = "Chemistry", Code = "CHEM1", StartDate = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new SubjectFilterDto { SearchTerm = "bio" };

            var result = await service.GetAllSubjectsAsync(filter);

            Assert.Equal(1, result.Data!.TotalCount);
            Assert.Equal("Biology", result.Data.Data.First().Name);
        }

        [Fact]
        public async Task GetAllSubjectsAsync_WhenIsActiveIsTrue_ReturnsOnlyActiveSubjects()
        {
            var context = await GetInMemoryDbContext();
            var now = DateTime.UtcNow;

            context.Subjects.AddRange(
                new Subject { Name = "Active1", StartDate = now.AddDays(-5) },
                new Subject { Name = "Active2", StartDate = now.AddDays(-5), EndDate = now.AddDays(10) },
                new Subject { Name = "Inactive1", StartDate = now.AddDays(-10), EndDate = now.AddDays(-1) },
                new Subject { Name = "Inactive2", StartDate = now.AddDays(5) }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new SubjectFilterDto { IsActive = true };

            var result = await service.GetAllSubjectsAsync(filter);

            Assert.Equal(2, result.Data!.TotalCount);
            Assert.Contains(result.Data.Data, s => s.Name == "Active1");
            Assert.Contains(result.Data.Data, s => s.Name == "Active2");
        }
    }
}