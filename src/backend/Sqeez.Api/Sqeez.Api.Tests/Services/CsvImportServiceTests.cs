using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Models.Academics;
using Sqeez.Api.Models.Import;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services;
using Sqeez.Api.Services.Interfaces;
using System.Text;
using Xunit;

namespace Sqeez.Api.Tests.Services
{
    public class CsvImportServiceTests
    {
        private async Task<SqeezDbContext> GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SqeezDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new SqeezDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return context;
        }

        private CsvImportService CreateService(
            SqeezDbContext context,
            Mock<ISchoolClassService> mockClassService,
            Mock<ISubjectService> mockSubjectService,
            Mock<IUserService> mockUserService)
        {
            var mockLogger = new Mock<ILogger<CsvImportService>>();
            return new CsvImportService(
                context,
                mockLogger.Object,
                mockClassService.Object,
                mockSubjectService.Object,
                mockUserService.Object);
        }

        private IFormFile CreateMockFile(string content, string fileName = "test.csv")
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, stream.Length, "file", fileName);
        }

        [Fact]
        public async Task ImportMasterFileAsync_WithNullFile_ReturnsBadRequest()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context, new Mock<ISchoolClassService>(), new Mock<ISubjectService>(), new Mock<IUserService>());

            var result = await service.ImportMasterFileAsync(null!);

            Assert.False(result.Success);
            Assert.Equal(Sqeez.Api.Enums.ServiceError.BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ImportMasterFileAsync_WithNonCsvFile_ReturnsBadRequest()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context, new Mock<ISchoolClassService>(), new Mock<ISubjectService>(), new Mock<IUserService>());
            var file = CreateMockFile("content", "test.txt");

            var result = await service.ImportMasterFileAsync(file);

            Assert.False(result.Success);
            Assert.Equal(Sqeez.Api.Enums.ServiceError.BadRequest, result.ErrorCode);
        }

        [Fact]
        public async Task ImportMasterFileAsync_WithValidCsv_CallsDependencies()
        {
            var context = await GetInMemoryDbContext();
            var mockClassService = new Mock<ISchoolClassService>();
            var mockSubjectService = new Mock<ISubjectService>();
            var mockUserService = new Mock<IUserService>();

            // Mock successful class creation
            var classResult = new BulkOperationResult<SchoolClassDto>
            {
                Created = new List<SchoolClassDto> { new SchoolClassDto(1, "ClassA", "2024", "A", null, null, 1, 1) },
                Existing = new List<SchoolClassDto>()
            };
            mockClassService.Setup(s => s.EnsureClassesExistAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(ServiceResult<BulkOperationResult<SchoolClassDto>>.Ok(classResult));

            // Mock successful subject creation
            var subjectResult = new BulkOperationResult<SubjectDto>
            {
                Created = new List<SubjectDto> { new SubjectDto(1, "Mathematics", "Math101", null, DateTime.UtcNow, null, null, null, null, null, 1, 1) },
                Existing = new List<SubjectDto>()
            };
            mockSubjectService.Setup(s => s.CreateSubjectsBulkAsync(It.IsAny<IEnumerable<Subject>>()))
                .ReturnsAsync(ServiceResult<BulkOperationResult<SubjectDto>>.Ok(subjectResult));

            // Mock successful student creation
            var studentResult = new BulkOperationResult<StudentDto>
            {
                Created = new List<StudentDto> { new StudentDto { Id = 1, Username = "john", Email = "john@sqeez.org", FirstName = "John", LastName = "Doe" } },
                Existing = new List<StudentDto>()
            };
            mockUserService.Setup(s => s.CreateStudentsBulkAsync(It.IsAny<IEnumerable<Student>>()))
                .ReturnsAsync(ServiceResult<BulkOperationResult<StudentDto>>.Ok(studentResult));

            var service = CreateService(context, mockClassService, mockSubjectService, mockUserService);

            var csvContent = "Class Name,Academic Year,Subject Name,Subject Code,First Name,Last Name,Email,Password\n" +
                             "ClassA,2024,Mathematics,Math101,John,Doe,john@sqeez.org,Heslo1122*\n";
            var file = CreateMockFile(csvContent);

            var result = await service.ImportMasterFileAsync(file);

            Assert.True(result.Success, result.ErrorMessage);
            Assert.NotNull(result.Data);

            Assert.Equal(3, result.Data!.RecordsImported); 

            mockClassService.Verify(s => s.EnsureClassesExistAsync(It.Is<IEnumerable<string>>(l => l.Contains("ClassA"))), Times.Once);
            mockSubjectService.Verify(s => s.CreateSubjectsBulkAsync(It.Is<IEnumerable<Subject>>(l => l.Any(subj => subj.Code == "Math101"))), Times.Once);
            mockUserService.Verify(s => s.CreateStudentsBulkAsync(It.Is<IEnumerable<Student>>(l => l.Any(stu => stu.Email == "john@sqeez.org"))), Times.Once);
        }

        [Fact]
        public async Task ImportMasterFileAsync_WhenRowsAreInvalid_ReturnsRowErrorsAndSkipsDependencies()
        {
            var context = await GetInMemoryDbContext();
            var mockClassService = new Mock<ISchoolClassService>();
            var mockSubjectService = new Mock<ISubjectService>();
            var mockUserService = new Mock<IUserService>();
            var service = CreateService(context, mockClassService, mockSubjectService, mockUserService);

            var csvContent = "Class Name,Academic Year,Subject Name,Subject Code,First Name,Last Name,Email,Password\n" +
                             "ClassA,2024,Mathematics,Math101,John,Doe,not-an-email,Heslo1122*\n";
            var file = CreateMockFile(csvContent);

            var result = await service.ImportMasterFileAsync(file);

            Assert.True(result.Success);
            Assert.Equal(0, result.Data!.RecordsImported);
            Assert.True(result.Data.HasRowErrors);
            Assert.Contains(result.Data.Errors, e => e.Contains("Row 2") && e.Contains("Invalid email format"));
            mockClassService.Verify(s => s.EnsureClassesExistAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
            mockSubjectService.Verify(s => s.CreateSubjectsBulkAsync(It.IsAny<IEnumerable<Subject>>()), Times.Never);
            mockUserService.Verify(s => s.CreateStudentsBulkAsync(It.IsAny<IEnumerable<Student>>()), Times.Never);
        }

        [Fact]
        public async Task ImportMasterFileAsync_WhenMixedValidity_ImportsValidRowsAndReportsInvalidRows()
        {
            var context = await GetInMemoryDbContext();
            var mockClassService = new Mock<ISchoolClassService>();
            var mockSubjectService = new Mock<ISubjectService>();
            var mockUserService = new Mock<IUserService>();

            mockClassService.Setup(s => s.EnsureClassesExistAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(ServiceResult<BulkOperationResult<SchoolClassDto>>.Ok(new BulkOperationResult<SchoolClassDto>
                {
                    Created = new List<SchoolClassDto> { new SchoolClassDto(1, "ClassA", "2024", "A", null, null, 0, 0) }
                }));

            mockSubjectService.Setup(s => s.CreateSubjectsBulkAsync(It.IsAny<IEnumerable<Subject>>()))
                .ReturnsAsync(ServiceResult<BulkOperationResult<SubjectDto>>.Ok(new BulkOperationResult<SubjectDto>
                {
                    Created = new List<SubjectDto> { new SubjectDto(1, "Mathematics", "Math101", null, DateTime.UtcNow, null, null, null, null, null, 0, 0) }
                }));

            mockUserService.Setup(s => s.CreateStudentsBulkAsync(It.IsAny<IEnumerable<Student>>()))
                .ReturnsAsync(ServiceResult<BulkOperationResult<StudentDto>>.Ok(new BulkOperationResult<StudentDto>
                {
                    Created = new List<StudentDto> { new StudentDto { Id = 1, Username = "jane", Email = "jane@sqeez.org" } }
                }));

            var service = CreateService(context, mockClassService, mockSubjectService, mockUserService);

            var csvContent = "Class Name,Academic Year,Subject Name,Subject Code,First Name,Last Name,Email,Password\n" +
                             "ClassA,2024,Mathematics,Math101,Jane,Doe,jane@sqeez.org,Heslo1122*\n" +
                             "ClassA,2024,Mathematics,Math101,Bad,User,bad-email,Heslo1122*\n";
            var file = CreateMockFile(csvContent);

            var result = await service.ImportMasterFileAsync(file);

            Assert.True(result.Success, result.ErrorMessage);
            Assert.Equal(3, result.Data!.RecordsImported);
            Assert.Single(result.Data.Errors);
            Assert.Contains("Row 3", result.Data.Errors.First());
            mockUserService.Verify(s => s.CreateStudentsBulkAsync(
                It.Is<IEnumerable<Student>>(students => students.Count() == 1 && students.First().Email == "jane@sqeez.org")),
                Times.Once);
        }
    }
}
