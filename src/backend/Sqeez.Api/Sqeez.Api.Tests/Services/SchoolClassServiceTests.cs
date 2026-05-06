using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Academics;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services;
using Xunit;

namespace Sqeez.Api.Tests.Services
{
    public class SchoolClassServiceTests
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

        private SchoolClassService CreateService(SqeezDbContext context)
        {
            var mockLogger = new Mock<ILogger<SchoolClassService>>();
            return new SchoolClassService(context, mockLogger.Object);
        }

        [Fact]
        public async Task GetClassByIdAsync_WhenExists_ReturnsSchoolClassDto()
        {
            var context = await GetInMemoryDbContext();
            var schoolClass = new SchoolClass { Name = "Class 1", AcademicYear = "2024", Section = "A" };
            context.SchoolClasses.Add(schoolClass);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetClassByIdAsync(schoolClass.Id);

            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.Data);
            Assert.Equal("Class 1", result.Data.Name);
            Assert.Equal("2024", result.Data.AcademicYear);
            Assert.Equal("A", result.Data.Section);
        }

        [Fact]
        public async Task GetAllClassesAsync_WithSearchTermAndTeacherFilter_ReturnsFilteredResults()
        {
            var context = await GetInMemoryDbContext();
            var teacher = new Teacher { Username = "Mr. Test", Role = UserRole.Teacher };
            context.Teachers.Add(teacher);

            context.SchoolClasses.AddRange(
                new SchoolClass { Name = "Alpha", Section = "A", AcademicYear = "2024", Teacher = teacher },
                new SchoolClass { Name = "Beta", Section = "B", AcademicYear = "2024" },
                new SchoolClass { Name = "Alpha", Section = "C", AcademicYear = "2023" }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Filter by SearchTerm "alpha" AND Teacher
            var filter = new SchoolClassFilterDto { SearchTerm = "alpha", TeacherId = teacher.Id };
            var result = await service.GetAllClassesAsync(filter);

            Assert.Equal(1, result.Data!.TotalCount);
            Assert.Equal("Alpha", result.Data.Data.First().Name);
            Assert.Equal("A", result.Data.Data.First().Section);
        }

        [Fact]
        public async Task CreateClassAsync_WhenValidTeacher_CreatesClass()
        {
            var context = await GetInMemoryDbContext();
            var teacher = new Teacher { Username = "ValidTeacher", Role = UserRole.Teacher };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new CreateSchoolClassDto
            (
                "New Class",
                "2024",
                "X",
                teacher.Id
            );

            var result = await service.CreateClassAsync(dto);

            Assert.Null(result.ErrorMessage);
            Assert.Equal("New Class", result.Data!.Name);
            Assert.Equal("ValidTeacher", result.Data.TeacherName);

            var dbClass = await context.SchoolClasses.FindAsync(result.Data.Id);
            Assert.NotNull(dbClass);
            Assert.Equal(teacher.Id, dbClass.Teacher?.Id);
        }

        [Fact]
        public async Task CreateClassAsync_WhenInvalidTeacher_ReturnsValidationFailed()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);

            var dto = new CreateSchoolClassDto("3. Bad Class", "2025-2026", "B", 999); // 999 does not exist

            var result = await service.CreateClassAsync(dto);

            Assert.Equal(ServiceError.ValidationFailed, result.ErrorCode);
            Assert.Contains("does not exist", result.ErrorMessage);
        }

        [Fact]
        public async Task PatchClassAsync_WhenSettingTeacherIdToZero_RemovesTeacher()
        {
            var context = await GetInMemoryDbContext();
            var teacher = new Teacher { Username = "OldTeacher" };
            var schoolClass = new SchoolClass { Name = "Class A", Teacher = teacher };
            context.SchoolClasses.Add(schoolClass);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var patchDto = new PatchSchoolClassDto( TeacherId: 0 ); // 0 means remove

            var result = await service.PatchClassAsync(schoolClass.Id, patchDto);

            Assert.Null(result.ErrorMessage);
            Assert.Null(result.Data!.TeacherId);
            Assert.Null(result.Data.TeacherName);

            var dbClass = await context.SchoolClasses.FindAsync(schoolClass.Id);
            Assert.Null(dbClass!.Teacher?.Id);
        }

        [Fact]
        public async Task PatchClassAsync_WhenTeacherIsAlreadyStudentOfClass_ReturnsValidationFailed()
        {
            var context = await GetInMemoryDbContext();
            var schoolClass = new SchoolClass { Name = "Class A", AcademicYear = "2025/2026", Section = "A" };
            var teacher = new Teacher
            {
                Username = "TeacherStudent",
                Role = UserRole.Teacher,
                SchoolClass = schoolClass
            };

            context.SchoolClasses.Add(schoolClass);
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var patchDto = new PatchSchoolClassDto(TeacherId: teacher.Id);

            var result = await service.PatchClassAsync(schoolClass.Id, patchDto);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.ValidationFailed, result.ErrorCode);
            Assert.Contains("already assigned", result.ErrorMessage);

            var dbClass = await context.SchoolClasses
                .Include(c => c.Teacher)
                .SingleAsync(c => c.Id == schoolClass.Id);
            Assert.Null(dbClass.Teacher);
        }

        [Fact]
        public async Task DeleteClassAsync_WhenExists_HardDeletesClass()
        {
            var context = await GetInMemoryDbContext();
            var schoolClass = new SchoolClass { Name = "To Be Deleted" };
            context.SchoolClasses.Add(schoolClass);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteClassAsync(schoolClass.Id);

            Assert.True(result.Success);

            var dbClass = await context.SchoolClasses.FindAsync(schoolClass.Id);
            Assert.Null(dbClass);
        }

        [Fact]
        public async Task AssignStudentsToClassAsync_WhenValid_AssignsClassIdToStudents()
        {
            var context = await GetInMemoryDbContext();
            var schoolClass = new SchoolClass { Name = "Homeroom" };
            var student1 = new Student { Username = "Student1" };
            var student2 = new Student { Username = "Student2" };

            context.SchoolClasses.Add(schoolClass);
            context.Students.AddRange(student1, student2);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new AssignStudentsDto(new List<long> { student1.Id, student2.Id });

            var result = await service.AssignStudentsToClassAsync(schoolClass.Id, dto);

            Assert.True(result.Success);

            var dbStudent1 = await context.Students.FindAsync(student1.Id);
            var dbStudent2 = await context.Students.FindAsync(student2.Id);

            Assert.Equal(schoolClass.Id, dbStudent1!.SchoolClassId);
            Assert.Equal(schoolClass.Id, dbStudent2!.SchoolClassId);
        }

        [Fact]
        public async Task AssignStudentsToClassAsync_WhenStudentInvalid_ReturnsValidationFailed()
        {
            var context = await GetInMemoryDbContext();
            var schoolClass = new SchoolClass { Name = "Homeroom" };
            var validStudent = new Student { Username = "Valid" };

            context.SchoolClasses.Add(schoolClass);
            context.Students.Add(validStudent);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            // Mix a valid ID with a completely fake ID (999)
            var dto = new AssignStudentsDto(new List<long> { validStudent.Id, 999 });

            var result = await service.AssignStudentsToClassAsync(schoolClass.Id, dto);

            Assert.Equal(ServiceError.ValidationFailed, result.ErrorCode);

            // Prove that the database wasn't modified because the transaction failed fast
            var dbStudent = await context.Students.FindAsync(validStudent.Id);
            Assert.Null(dbStudent!.SchoolClassId);
        }

        [Fact]
        public async Task AssignStudentsToClassAsync_WhenStudentIdIsClassTeacher_ReturnsValidationFailed()
        {
            var context = await GetInMemoryDbContext();
            var teacher = new Teacher { Username = "ClassTeacher", Role = UserRole.Teacher };
            var schoolClass = new SchoolClass { Name = "Homeroom", Teacher = teacher };

            context.SchoolClasses.Add(schoolClass);
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new AssignStudentsDto(new List<long> { teacher.Id });

            var result = await service.AssignStudentsToClassAsync(schoolClass.Id, dto);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.ValidationFailed, result.ErrorCode);
            Assert.Contains("teacher cannot also be assigned as a student", result.ErrorMessage);

            var dbTeacher = await context.Teachers.FindAsync(teacher.Id);
            Assert.Null(dbTeacher!.SchoolClassId);
        }

        [Fact]
        public async Task RemoveStudentsFromClassAsync_WhenValid_SetsClassIdToNull()
        {
            var context = await GetInMemoryDbContext();
            var schoolClass = new SchoolClass { Name = "Homeroom" };
            var studentToKeep = new Student { Username = "KeepMe", SchoolClass = schoolClass };
            var studentToRemove = new Student { Username = "RemoveMe", SchoolClass = schoolClass };

            context.SchoolClasses.Add(schoolClass);
            context.Students.AddRange(studentToKeep, studentToRemove);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new RemoveStudentsDto(new List<long> { studentToRemove.Id });

            var result = await service.RemoveStudentsFromClassAsync(schoolClass.Id, dto);

            Assert.True(result.Success);

            var dbStudentToKeep = await context.Students.FindAsync(studentToKeep.Id);
            var dbStudentToRemove = await context.Students.FindAsync(studentToRemove.Id);

            Assert.Equal(schoolClass.Id, dbStudentToKeep!.SchoolClassId); // Left alone
            Assert.Null(dbStudentToRemove!.SchoolClassId); // Successfully removed
        }

        [Fact]
        public async Task EnsureClassesExistAsync_WhenMixedClasses_CreatesMissingAndReturnsExisting()
        {
            var context = await GetInMemoryDbContext();
            var existingClass = new SchoolClass { Name = "ExistingClass", AcademicYear = "2024", Section = "A" };
            context.SchoolClasses.Add(existingClass);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var classNames = new List<string> { "ExistingClass", "NewClass" };

            var result = await service.EnsureClassesExistAsync(classNames);

            Assert.True(result.Success);
            Assert.Single(result.Data!.Existing);
            Assert.Single(result.Data.Created);

            Assert.Equal("ExistingClass", result.Data.Existing.First().Name);
            Assert.Equal("NewClass", result.Data.Created.First().Name);

            var dbNewClass = await context.SchoolClasses.FirstOrDefaultAsync(c => c.Name == "NewClass");
            Assert.NotNull(dbNewClass);
        }
    }
}
