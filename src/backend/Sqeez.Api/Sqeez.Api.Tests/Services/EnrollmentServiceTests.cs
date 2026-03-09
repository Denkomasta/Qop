using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Academics;
using Sqeez.Api.Models.QuizSystem;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services;
using Xunit;

namespace Sqeez.Api.Tests.Services
{
    public class EnrollmentServiceTests
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

        private EnrollmentService CreateService(SqeezDbContext context)
        {
            var mockLogger = new Mock<ILogger<EnrollmentService>>();
            return new EnrollmentService(context, mockLogger.Object);
        }

        [Fact]
        public async Task GetEnrollmentByIdAsync_WhenExists_ReturnsEnrollmentDto()
        {
            var context = await GetInMemoryDbContext();
            var student = new Student { Username = "TestStudent" };
            var subject = new Subject { Name = "TestSubject" };
            var enrollment = new Enrollment { Student = student, Subject = subject, EnrolledAt = DateTime.UtcNow, Mark = 5 };

            context.Enrollments.Add(enrollment);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetEnrollmentByIdAsync(enrollment.Id);

            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal(5, result.Data.Mark);
            Assert.Equal(student.Id, result.Data.StudentId);
        }

        [Fact]
        public async Task GetAllEnrollmentsAsync_WhenIsActiveFilterApplied_ReturnsCorrectRecords()
        {
            var context = await GetInMemoryDbContext();
            var student = new Student { Username = "TestStudent" };
            var subject = new Subject { Name = "TestSubject" };

            context.Enrollments.AddRange(
                new Enrollment { Student = student, Subject = subject, EnrolledAt = DateTime.UtcNow, ArchivedAt = null }, // Active
                new Enrollment { Student = student, Subject = subject, EnrolledAt = DateTime.UtcNow, ArchivedAt = DateTime.UtcNow } // Inactive
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Test Active Only
            var activeFilter = new EnrollmentFilterDto { IsActive = true };
            var activeResult = await service.GetAllEnrollmentsAsync(activeFilter);

            Assert.Equal(1, activeResult.Data!.TotalCount);
            Assert.Null(activeResult.Data.Data.First().ArchivedAt);

            // Test Inactive Only
            var inactiveFilter = new EnrollmentFilterDto { IsActive = false };
            var inactiveResult = await service.GetAllEnrollmentsAsync(inactiveFilter);

            Assert.Equal(1, inactiveResult.Data!.TotalCount);
            Assert.NotNull(inactiveResult.Data.Data.First().ArchivedAt);
        }

        [Fact]
        public async Task PatchEnrollmentAsync_WhenSettingValidMark_UpdatesMark()
        {
            var context = await GetInMemoryDbContext();
            var enrollment = new Enrollment { Student = new Student(), Subject = new Subject(), EnrolledAt = DateTime.UtcNow };
            context.Enrollments.Add(enrollment);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var patchDto = new PatchEnrollmentDto(Mark: 3);

            var result = await service.PatchEnrollmentAsync(enrollment.Id, patchDto);

            Assert.Null(result.ErrorMessage);
            Assert.Equal(3, result.Data!.Mark);

            var dbRecord = await context.Enrollments.FindAsync(enrollment.Id);
            Assert.Equal(3, dbRecord!.Mark);
        }

        [Fact]
        public async Task PatchEnrollmentAsync_WhenRemovingMark_SetsMarkToNull()
        {
            var context = await GetInMemoryDbContext();
            var enrollment = new Enrollment { Student = new Student(), Subject = new Subject(), EnrolledAt = DateTime.UtcNow, Mark = 5 };
            context.Enrollments.Add(enrollment);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Simulate teacher clicking "Clear Grade"
            var patchDto = new PatchEnrollmentDto(Mark: null, RemoveMark: true);

            var result = await service.PatchEnrollmentAsync(enrollment.Id, patchDto);

            Assert.Null(result.ErrorMessage);
            Assert.Null(result.Data!.Mark);
        }

        [Fact]
        public async Task PatchEnrollmentAsync_WhenMarkIsInvalid_ReturnsValidationFailed()
        {
            var context = await GetInMemoryDbContext();
            var enrollment = new Enrollment { Student = new Student(), Subject = new Subject() };
            context.Enrollments.Add(enrollment);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var patchDto = new PatchEnrollmentDto(Mark: 99); // Invalid mark

            var result = await service.PatchEnrollmentAsync(enrollment.Id, patchDto);

            Assert.Equal(ServiceError.ValidationFailed, result.ErrorCode);
        }

        [Fact]
        public async Task DeleteEnrollmentAsync_WhenNoQuizAttempts_HardDeletes()
        {
            var context = await GetInMemoryDbContext();
            var enrollment = new Enrollment { Student = new Student(), Subject = new Subject() };
            context.Enrollments.Add(enrollment);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteEnrollmentAsync(enrollment.Id);

            Assert.True(result.Success);

            var deletedRecord = await context.Enrollments.FindAsync(enrollment.Id);
            Assert.Null(deletedRecord); // Completely gone
        }

        [Fact]
        public async Task DeleteEnrollmentAsync_WhenHasQuizAttempts_SoftDeletes()
        {
            var context = await GetInMemoryDbContext();
            var enrollment = new Enrollment { Student = new Student(), Subject = new Subject() };

            // Add a mock quiz attempt so it triggers soft deletion
            enrollment.QuizAttempts.Add(new QuizAttempt { TotalScore = 10 });
            context.Enrollments.Add(enrollment);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteEnrollmentAsync(enrollment.Id);

            Assert.True(result.Success);

            var archivedRecord = await context.Enrollments.FindAsync(enrollment.Id);
            Assert.NotNull(archivedRecord); // Still exists
            Assert.NotNull(archivedRecord.ArchivedAt); // But is archived
        }

        [Fact]
        public async Task EnrollStudentsInSubjectAsync_WhenValid_EnrollsNewStudentsOnly()
        {
            var context = await GetInMemoryDbContext();
            var subject = new Subject { Name = "Science" };
            var existingStudent = new Student { Username = "AlreadyHere" };
            var newStudent = new Student { Username = "NewGuy" };

            context.Subjects.Add(subject);
            context.Students.AddRange(existingStudent, newStudent);

            // Existing student is already enrolled
            context.Enrollments.Add(new Enrollment { Student = existingStudent, Subject = subject });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new AssignStudentsDto(new List<long> { existingStudent.Id, newStudent.Id });

            var result = await service.EnrollStudentsInSubjectAsync(subject.Id, dto);

            Assert.True(result.Success);

            // Should have 2 enrollments total now, didn't create a duplicate for existingStudent
            var totalEnrollments = await context.Enrollments.CountAsync(e => e.SubjectId == subject.Id);
            Assert.Equal(2, totalEnrollments);
        }

        [Fact]
        public async Task UnenrollStudentsFromSubjectAsync_WhenValid_ArchivesStudents()
        {
            var context = await GetInMemoryDbContext();
            var subject = new Subject { Name = "History" };
            var student = new Student { Username = "DropOut" };
            var enrollment = new Enrollment { Student = student, Subject = subject, EnrolledAt = DateTime.UtcNow };

            // Give them a quiz attempt so they soft-delete
            enrollment.QuizAttempts.Add(new QuizAttempt { TotalScore = 5 });

            context.Subjects.Add(subject);
            context.Students.Add(student);
            context.Enrollments.Add(enrollment);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new RemoveStudentsDto(new List<long> { student.Id });

            var result = await service.UnenrollStudentsFromSubjectAsync(subject.Id, dto);

            Assert.True(result.Success);

            var dbEnrollment = await context.Enrollments.FirstOrDefaultAsync(e => e.Id == enrollment.Id);
            Assert.NotNull(dbEnrollment);
            Assert.NotNull(dbEnrollment.ArchivedAt); // Successfully Soft Deleted
        }
    }
}