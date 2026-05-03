using System.Net;
using System.Net.Http.Json;
using Moq;
using Sqeez.Api.DTOs;

namespace Sqeez.Api.Tests.Integration
{
    public class SystemConfigControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public SystemConfigControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _factory.ResetMocks();
        }

        [Fact]
        public async Task GetConfig_IsPublicAndCallsConfigService()
        {
            _factory.SystemConfigServiceMock
                .Setup(service => service.GetConfigAsync())
                .ReturnsAsync(ServiceResult<SystemConfigDto>.Ok(
                    new SystemConfigDto("Sqeez", "/logo.png", "support@sqeez.test", "en", "2025/2026", true, true, 10, 20, 3)));

            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/system-config");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _factory.SystemConfigServiceMock.Verify(service => service.GetConfigAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateConfig_WithInvalidEmail_ReturnsBadRequestBeforeCallingService()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, "1");
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, "Admin");

            var response = await client.PatchAsJsonAsync("/api/system-config", new
            {
                supportEmail = "not-email"
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _factory.SystemConfigServiceMock.Verify(
                service => service.UpdateConfigAsync(It.IsAny<UpdateSystemConfigDto>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateConfig_AsAdmin_CallsConfigService()
        {
            _factory.SystemConfigServiceMock
                .Setup(service => service.UpdateConfigAsync(It.Is<UpdateSystemConfigDto>(dto =>
                    dto.SchoolName == "Sqeez Academy" &&
                    dto.MaxActiveSessionsPerUser == 5)))
                .ReturnsAsync(ServiceResult<SystemConfigDto>.Ok(
                    new SystemConfigDto("Sqeez Academy", "/logo.png", "support@sqeez.test", "en", "2025/2026", true, true, 10, 20, 5)));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, "1");
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, "Admin");

            var response = await client.PatchAsJsonAsync("/api/system-config", new
            {
                schoolName = "Sqeez Academy",
                maxActiveSessionsPerUser = 5
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _factory.SystemConfigServiceMock.Verify(
                service => service.UpdateConfigAsync(It.IsAny<UpdateSystemConfigDto>()),
                Times.Once);
        }
    }
}
