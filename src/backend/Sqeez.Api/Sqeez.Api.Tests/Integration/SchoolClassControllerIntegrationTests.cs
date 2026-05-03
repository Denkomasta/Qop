using System.Net;
using System.Net.Http.Json;
using Moq;
using Sqeez.Api.DTOs;

namespace Sqeez.Api.Tests.Integration
{
    public class SchoolClassControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public SchoolClassControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _factory.ResetMocks();
        }

        [Fact]
        public async Task CreateClass_AsAdmin_ReturnsCreatedAndCallsService()
        {
            _factory.SchoolClassServiceMock
                .Setup(service => service.CreateClassAsync(It.Is<CreateSchoolClassDto>(dto =>
                    dto.Name == "4.A" &&
                    dto.AcademicYear == "2025/2026" &&
                    dto.Section == "A" &&
                    dto.TeacherId == 8)))
                .ReturnsAsync(ServiceResult<SchoolClassDto>.Ok(
                    new SchoolClassDto(12, "4.A", "2025/2026", "A", 8, "Teacher", 0, 0)));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, "1");
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, "Admin");

            var response = await client.PostAsJsonAsync("/api/classes", new
            {
                name = "4.A",
                academicYear = "2025/2026",
                section = "A",
                teacherId = 8
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Contains("/api/classes/12", response.Headers.Location?.ToString());
            _factory.SchoolClassServiceMock.Verify(service => service.CreateClassAsync(It.IsAny<CreateSchoolClassDto>()), Times.Once);
        }

        [Fact]
        public async Task AssignStudents_WithTooManyIds_ReturnsBadRequestBeforeCallingService()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, "1");
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, "Admin");

            var response = await client.PostAsJsonAsync("/api/classes/12/students", new
            {
                studentIds = Enumerable.Range(1, 1001).ToList()
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _factory.SchoolClassServiceMock.Verify(
                service => service.AssignStudentsToClassAsync(It.IsAny<long>(), It.IsAny<AssignStudentsDto>()),
                Times.Never);
        }
    }
}
