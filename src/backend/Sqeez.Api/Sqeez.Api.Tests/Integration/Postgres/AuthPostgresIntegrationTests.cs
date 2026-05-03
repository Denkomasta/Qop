using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sqeez.Api.Data;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.System;
using Sqeez.Api.Models.Users;
using BC = BCrypt.Net.BCrypt;

namespace Sqeez.Api.Tests.Integration.Postgres
{
    [Collection(PostgresIntegrationTestCollection.CollectionName)]
    public class AuthPostgresIntegrationTests
    {
        private readonly PostgresIntegrationTestFixture _fixture;

        public AuthPostgresIntegrationTests(PostgresIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [DockerAvailableFact]
        public async Task Migrations_ApplyAgainstPostgreSql()
        {
            await _fixture.ResetDatabaseAsync();

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();

            Assert.True(await dbContext.Database.CanConnectAsync());
            Assert.True(await dbContext.SystemConfigs.AnyAsync(config => config.Id == 1));
        }

        [DockerAvailableFact]
        public async Task Register_WithPublicRegistrationEnabled_PersistsUnverifiedStudent()
        {
            await _fixture.ResetDatabaseAsync();
            await UpdateSystemConfigAsync(config =>
            {
                config.AllowPublicRegistration = true;
                config.RequireEmailVerification = true;
            });

            var client = _fixture.Factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                firstName = "Jana",
                lastName = "Novakova",
                username = "jana",
                email = "Jana@Sqeez.Test",
                password = "StrongPassword123!",
                rememberMe = true
            });

            await AssertStatusCodeAsync(HttpStatusCode.OK, response);

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var user = await dbContext.Students.SingleAsync(student => student.Email == "jana@sqeez.test");

            Assert.Equal("jana", user.Username);
            Assert.Equal(UserRole.Student, user.Role);
            Assert.False(user.IsEmailVerified);
            Assert.False(string.IsNullOrWhiteSpace(user.EmailVerificationToken));
            Assert.NotNull(user.EmailVerificationTokenExpiry);
        }

        [DockerAvailableFact]
        public async Task Login_WithVerifiedUser_CreatesSessionAndSetsCookies()
        {
            await _fixture.ResetDatabaseAsync();
            var userId = await SeedVerifiedStudentAsync("login@sqeez.test", "LoginUser", "StrongPassword123!");

            var client = _fixture.Factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "login@sqeez.test",
                password = "StrongPassword123!",
                rememberMe = true
            });

            await AssertStatusCodeAsync(HttpStatusCode.OK, response);
            Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
            Assert.Contains(cookies, cookie => cookie.StartsWith("sqeez_access_token="));
            Assert.Contains(cookies, cookie => cookie.StartsWith("sqeez_refresh_token="));

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var session = await dbContext.UserSessions.SingleAsync(session => session.UserId == userId);

            Assert.False(session.IsRevoked);
            Assert.True(session.ExpiresAt > DateTime.UtcNow);
        }

        [DockerAvailableFact]
        public async Task Refresh_WithValidSession_RevokesOldSessionAndCreatesNewOne()
        {
            await _fixture.ResetDatabaseAsync();
            var userId = await SeedVerifiedStudentAsync("refresh@sqeez.test", "RefreshUser", "StrongPassword123!");
            await SeedRefreshSessionAsync(userId, "old-refresh-token");

            var client = _fixture.Factory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
            request.Headers.Add("Cookie", "sqeez_refresh_token=old-refresh-token");

            var response = await client.SendAsync(request);

            await AssertStatusCodeAsync(HttpStatusCode.OK, response);
            Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
            Assert.Contains(cookies, cookie => cookie.StartsWith("sqeez_access_token="));
            Assert.Contains(cookies, cookie => cookie.StartsWith("sqeez_refresh_token="));

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var sessions = await dbContext.UserSessions
                .Where(session => session.UserId == userId)
                .OrderBy(session => session.CreatedAt)
                .ToListAsync();

            Assert.Equal(2, sessions.Count);
            Assert.True(sessions.Single(session => session.RefreshToken == "old-refresh-token").IsRevoked);
            Assert.Contains(sessions, session => session.RefreshToken != "old-refresh-token" && !session.IsRevoked);
        }

        [DockerAvailableFact]
        public async Task CurrentUser_WithRealAccessTokenCookie_ReturnsUserProfile()
        {
            await _fixture.ResetDatabaseAsync();
            await SeedVerifiedStudentAsync("me@sqeez.test", "MeUser", "StrongPassword123!");

            var client = _fixture.Factory.CreateClient();
            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "me@sqeez.test",
                password = "StrongPassword123!",
                rememberMe = false
            });
            var accessToken = GetCookieValue(loginResponse, "sqeez_access_token");

            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
            request.Headers.Add("Cookie", $"sqeez_access_token={accessToken}");

            var response = await client.SendAsync(request);
            await AssertStatusCodeAsync(HttpStatusCode.OK, response);
            var body = await response.Content.ReadAsStringAsync();

            Assert.Contains("\"username\":\"MeUser\"", body);
            Assert.Contains("\"role\":\"Student\"", body);
        }

        private async Task UpdateSystemConfigAsync(Action<SystemConfig> update)
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var config = await dbContext.SystemConfigs.SingleAsync(config => config.Id == 1);
            update(config);
            await dbContext.SaveChangesAsync();
        }

        private async Task<long> SeedVerifiedStudentAsync(string email, string username, string password)
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();

            var user = new Student
            {
                FirstName = "Test",
                LastName = "Student",
                Username = username,
                Email = email,
                PasswordHash = BC.HashPassword(password),
                Role = UserRole.Student,
                IsEmailVerified = true,
                LastSeen = DateTime.UtcNow.AddHours(-1)
            };

            dbContext.Students.Add(user);
            await dbContext.SaveChangesAsync();
            return user.Id;
        }

        private async Task SeedRefreshSessionAsync(long userId, string refreshToken)
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();

            dbContext.UserSessions.Add(new UserSession
            {
                UserId = userId,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            });

            await dbContext.SaveChangesAsync();
        }

        private static string GetCookieValue(HttpResponseMessage response, string name)
        {
            Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));

            var cookie = cookies.First(value => value.StartsWith($"{name}="));
            var value = cookie.Split(';', 2)[0].Split('=', 2)[1];

            Assert.False(string.IsNullOrWhiteSpace(value));
            return value;
        }

        private static async Task AssertStatusCodeAsync(HttpStatusCode expected, HttpResponseMessage response)
        {
            if (response.StatusCode == expected)
            {
                return;
            }

            var body = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Expected status code {expected}, got {response.StatusCode}. Response body: {body}");
        }
    }
}
