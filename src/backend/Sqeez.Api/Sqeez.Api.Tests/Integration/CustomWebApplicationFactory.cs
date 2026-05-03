using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.Services.AuthService;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IAuthService> AuthServiceMock { get; } = new();
        public Mock<IEnrollmentService> EnrollmentServiceMock { get; } = new();
        public Mock<ISchoolClassService> SchoolClassServiceMock { get; } = new();
        public Mock<ISubjectService> SubjectServiceMock { get; } = new();
        public Mock<IQuizService> QuizServiceMock { get; } = new();
        public Mock<IQuizQuestionService> QuizQuestionServiceMock { get; } = new();
        public Mock<IQuizOptionService> QuizOptionServiceMock { get; } = new();
        public Mock<IQuizAttemptService> QuizAttemptServiceMock { get; } = new();
        public Mock<IMediaAssetService> MediaAssetServiceMock { get; } = new();
        public Mock<IFileStorageService> FileStorageServiceMock { get; } = new();
        public Mock<IBadgeService> BadgeServiceMock { get; } = new();
        public Mock<ISystemConfigService> SystemConfigServiceMock { get; } = new();
        public Mock<IUserService> UserServiceMock { get; } = new();
        public Mock<ICsvImportService> CsvImportServiceMock { get; } = new();
        public Mock<IQuizStatisticsService> QuizStatisticsServiceMock { get; } = new();

        public CustomWebApplicationFactory()
        {
            Environment.SetEnvironmentVariable("TokenKey", "integration-test-token-key-with-enough-length");
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Host=localhost;Database=sqeez_integration_tests");
            Environment.SetEnvironmentVariable("FrontendUrl", "http://localhost:3000");
        }

        public void ResetMocks()
        {
            AuthServiceMock.Reset();
            EnrollmentServiceMock.Reset();
            SchoolClassServiceMock.Reset();
            SubjectServiceMock.Reset();
            QuizServiceMock.Reset();
            QuizQuestionServiceMock.Reset();
            QuizOptionServiceMock.Reset();
            QuizAttemptServiceMock.Reset();
            MediaAssetServiceMock.Reset();
            FileStorageServiceMock.Reset();
            BadgeServiceMock.Reset();
            SystemConfigServiceMock.Reset();
            UserServiceMock.Reset();
            CsvImportServiceMock.Reset();
            QuizStatisticsServiceMock.Reset();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TokenKey"] = "integration-test-token-key-with-enough-length",
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=sqeez_integration_tests",
                    ["FrontendUrl"] = "http://localhost:3000"
                });
            });

            builder.ConfigureTestServices(services =>
            {
                var inMemoryEntityFrameworkProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                services.RemoveAll<DbContextOptions<SqeezDbContext>>();
                services.AddDbContext<SqeezDbContext>(options =>
                    options
                        .UseInMemoryDatabase($"SqeezIntegrationTests-{Guid.NewGuid()}")
                        .UseInternalServiceProvider(inMemoryEntityFrameworkProvider));

                services.RemoveAll<IAuthService>();
                services.AddScoped(_ => AuthServiceMock.Object);

                services.RemoveAll<IEnrollmentService>();
                services.AddScoped(_ => EnrollmentServiceMock.Object);

                services.RemoveAll<ISchoolClassService>();
                services.AddScoped(_ => SchoolClassServiceMock.Object);

                services.RemoveAll<ISubjectService>();
                services.AddScoped(_ => SubjectServiceMock.Object);

                services.RemoveAll<IQuizService>();
                services.AddScoped(_ => QuizServiceMock.Object);

                services.RemoveAll<IQuizQuestionService>();
                services.AddScoped(_ => QuizQuestionServiceMock.Object);

                services.RemoveAll<IQuizOptionService>();
                services.AddScoped(_ => QuizOptionServiceMock.Object);

                services.RemoveAll<IQuizAttemptService>();
                services.AddScoped(_ => QuizAttemptServiceMock.Object);

                services.RemoveAll<IMediaAssetService>();
                services.AddScoped(_ => MediaAssetServiceMock.Object);

                services.RemoveAll<IFileStorageService>();
                services.AddScoped(_ => FileStorageServiceMock.Object);

                services.RemoveAll<IBadgeService>();
                services.AddScoped(_ => BadgeServiceMock.Object);

                services.RemoveAll<ISystemConfigService>();
                services.AddScoped(_ => SystemConfigServiceMock.Object);

                services.RemoveAll<IUserService>();
                services.AddScoped(_ => UserServiceMock.Object);

                services.RemoveAll<ICsvImportService>();
                services.AddScoped(_ => CsvImportServiceMock.Object);

                services.RemoveAll<IQuizStatisticsService>();
                services.AddScoped(_ => QuizStatisticsServiceMock.Object);

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthenticationHandler.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.AuthenticationScheme,
                    _ => { });
            });
        }
    }

    public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string AuthenticationScheme = "Test";
        public const string UserIdHeader = "X-Test-UserId";
        public const string RoleHeader = "X-Test-Role";

        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(UserIdHeader, out var userId))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var role = Request.Headers.TryGetValue(RoleHeader, out var roleHeader)
                ? roleHeader.ToString()
                : "Student";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Name, "integration-test-user")
            };

            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
