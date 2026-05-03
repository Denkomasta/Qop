using System.Net;
using System.Text.Json;

namespace Sqeez.Api.Tests.Integration
{
    public class OpenApiSchemaIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public OpenApiSchemaIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task OpenApiSchema_ExposesValidationLimitsForOrval()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/openapi/v1.json");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var schemas = document.RootElement
                .GetProperty("components")
                .GetProperty("schemas");

            var register = schemas.GetProperty("RegisterDTO").GetProperty("properties");
            Assert.Equal(20, register.GetProperty("username").GetProperty("maxLength").GetInt32());
            Assert.Equal(3, register.GetProperty("username").GetProperty("minLength").GetInt32());
            Assert.True(register.GetProperty("username").TryGetProperty("pattern", out _));
            Assert.Equal(128, register.GetProperty("password").GetProperty("maxLength").GetInt32());
            Assert.Equal(8, register.GetProperty("password").GetProperty("minLength").GetInt32());
            Assert.True(register.GetProperty("password").TryGetProperty("pattern", out _));

            var patchEnrollment = schemas.GetProperty("PatchEnrollmentDto").GetProperty("properties");
            Assert.Equal(1, patchEnrollment.GetProperty("mark").GetProperty("minimum").GetInt32());
            Assert.Equal(5, patchEnrollment.GetProperty("mark").GetProperty("maximum").GetInt32());

            var createBadgeRule = schemas.GetProperty("CreateBadgeRuleDto").GetProperty("properties");
            Assert.Equal(0, createBadgeRule.GetProperty("targetValue").GetProperty("minimum").GetInt32());
            Assert.Equal(1000000, createBadgeRule.GetProperty("targetValue").GetProperty("maximum").GetInt32());
        }
    }
}
