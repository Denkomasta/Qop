using System.Net;
using System.Net.Http.Json;
using Moq;
using Sqeez.Api.DTOs;
using Sqeez.Api.Services.AuthService;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Tests.Integration
{
    public class ValidationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ValidationIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _factory.ResetMocks();
        }

        [Fact]
        public async Task Register_WithInvalidDto_ReturnsBadRequestBeforeCallingService()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                firstName = "Jana",
                lastName = "Novakova",
                username = "ab",
                email = "not-an-email",
                password = "weak",
                rememberMe = false
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _factory.AuthServiceMock.Verify(
                service => service.RegisterAsync(It.IsAny<RegisterDTO>()),
                Times.Never);
        }

        [Fact]
        public async Task Login_WithTooLongPassword_ReturnsBadRequestBeforeCallingService()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "student@sqeez.test",
                password = new string('a', 129),
                rememberMe = true
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _factory.AuthServiceMock.Verify(
                service => service.LoginAsync(It.IsAny<LoginDTO>()),
                Times.Never);
        }

        [Fact]
        public async Task PatchEnrollment_WithInvalidMark_ReturnsBadRequestBeforeCallingService()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, "42");
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, "Teacher");

            var response = await client.PatchAsJsonAsync("/api/enrollments/10", new
            {
                mark = 6,
                removeMark = false
            });

            var body = await response.Content.ReadAsStringAsync();
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest, body);
            _factory.EnrollmentServiceMock.Verify(
                service => service.PatchEnrollmentAsync(
                    It.IsAny<long>(),
                    It.IsAny<PatchEnrollmentDto>(),
                    It.IsAny<long>()),
                Times.Never);
        }
    }
}
