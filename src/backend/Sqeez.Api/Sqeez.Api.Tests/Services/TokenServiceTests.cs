using Microsoft.Extensions.Configuration;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services.TokenService;
using Xunit;

namespace Sqeez.Api.Tests.Services
{
    public class TokenServiceTests
    {
        private TokenService CreateService(string tokenKey = "SuperSecretKeyThatIsAtLeast64BytesLongSoItPassesTheCheck_MakeItLonger_Yes_StillLonger_Okay")
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"TokenKey", tokenKey}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            return new TokenService(configuration);
        }

        [Fact]
        public void CreateToken_ReturnsValidToken()
        {
            var service = CreateService();
            var user = new Student { Id = 1, Username = "testuser", Role = UserRole.Student };

            var result = service.CreateToken(user);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);
        }

        [Fact]
        public void CreateToken_WhenExceptionOccurs_ReturnsFailure()
        {
            // Providing an invalid short key will cause SymmetricSecurityKey to throw on token creation
            var service = CreateService("shortkey");
            var user = new Student { Id = 1, Username = "testuser", Role = UserRole.Student };

            var result = service.CreateToken(user);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.InternalError, result.ErrorCode);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public void GenerateRefreshToken_ReturnsBase64String()
        {
            var service = CreateService();

            var token = service.GenerateRefreshToken();

            Assert.NotNull(token);
            Assert.NotEmpty(token);
            // Verify it is base64
            var bytes = Convert.FromBase64String(token);
            Assert.Equal(32, bytes.Length);
        }
    }
}
