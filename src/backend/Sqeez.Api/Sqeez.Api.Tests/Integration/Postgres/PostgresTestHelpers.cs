using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Sqeez.Api.Data;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services.TokenService;

namespace Sqeez.Api.Tests.Integration.Postgres
{
    internal static class PostgresTestHelpers
    {
        public static HttpClient CreateAuthenticatedClient(PostgresIntegrationTestFixture fixture, Student user)
        {
            var client = fixture.Factory.CreateClient();

            using var scope = fixture.Factory.Services.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var tokenResult = tokenService.CreateToken(user);

            Assert.True(tokenResult.Success, tokenResult.ErrorMessage);
            client.DefaultRequestHeaders.Add("Cookie", $"sqeez_access_token={tokenResult.Data}");

            return client;
        }

        public static async Task<Student> SeedStudentAsync(SqeezDbContext dbContext, string prefix = "student")
        {
            var user = new Student
            {
                FirstName = "Live",
                LastName = "Student",
                Username = UniqueValue(prefix, 20),
                Email = $"{UniqueValue(prefix, 24)}@sqeez.test",
                PasswordHash = "not-used",
                Role = UserRole.Student,
                IsEmailVerified = true,
                LastSeen = DateTime.UtcNow
            };

            dbContext.Students.Add(user);
            await dbContext.SaveChangesAsync();
            return user;
        }

        public static async Task<Teacher> SeedTeacherAsync(SqeezDbContext dbContext, string prefix = "teacher")
        {
            var user = new Teacher
            {
                FirstName = "Live",
                LastName = "Teacher",
                Username = UniqueValue(prefix, 20),
                Email = $"{UniqueValue(prefix, 24)}@sqeez.test",
                PasswordHash = "not-used",
                Role = UserRole.Teacher,
                IsEmailVerified = true,
                LastSeen = DateTime.UtcNow,
                Department = "Integration"
            };

            dbContext.Teachers.Add(user);
            await dbContext.SaveChangesAsync();
            return user;
        }

        public static async Task<Admin> SeedAdminAsync(SqeezDbContext dbContext, string prefix = "admin")
        {
            var user = new Admin
            {
                FirstName = "Live",
                LastName = "Admin",
                Username = UniqueValue(prefix, 20),
                Email = $"{UniqueValue(prefix, 24)}@sqeez.test",
                PasswordHash = "not-used",
                Role = UserRole.Admin,
                IsEmailVerified = true,
                LastSeen = DateTime.UtcNow
            };

            dbContext.Admins.Add(user);
            await dbContext.SaveChangesAsync();
            return user;
        }

        public static async Task AssertStatusCodeAsync(HttpStatusCode expected, HttpResponseMessage response)
        {
            if (response.StatusCode == expected)
            {
                return;
            }

            var body = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Expected status code {expected}, got {response.StatusCode}. Response body: {body}");
        }

        public static string UniqueValue(string prefix, int maxLength)
        {
            var value = $"{prefix}-{Guid.NewGuid():N}";
            return value[..Math.Min(value.Length, maxLength)];
        }
    }
}
