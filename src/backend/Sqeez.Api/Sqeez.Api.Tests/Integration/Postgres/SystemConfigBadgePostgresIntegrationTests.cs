using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sqeez.Api.Data;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Gamification;
using Sqeez.Api.Models.Users;

namespace Sqeez.Api.Tests.Integration.Postgres
{
    [Collection(PostgresIntegrationTestCollection.CollectionName)]
    public class SystemConfigBadgePostgresIntegrationTests
    {
        private readonly PostgresIntegrationTestFixture _fixture;

        public SystemConfigBadgePostgresIntegrationTests(PostgresIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [DockerAvailableFact]
        public async Task GetConfig_ReturnsSeededDefaults()
        {
            await _fixture.ResetDatabaseAsync();
            var client = _fixture.Factory.CreateClient();

            var response = await client.GetAsync("/api/system-config");

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, response);
            var body = await response.Content.ReadAsStringAsync();

            Assert.Contains("\"schoolName\":\"Sqeez\"", body);
            Assert.Contains("\"supportEmail\":\"support@sqeez.org\"", body);
            Assert.Contains("\"maxActiveSessionsPerUser\":3", body);
        }

        [DockerAvailableFact]
        public async Task UpdateConfig_AsAdmin_PersistsValuesAndUpdatesPublicRead()
        {
            await _fixture.ResetDatabaseAsync();
            var admin = await AddAdminAsync();
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, admin);

            var updateResponse = await client.PatchAsJsonAsync("/api/system-config", new
            {
                schoolName = "Sqeez Live Academy",
                supportEmail = "support-live@sqeez.test",
                defaultLanguage = "cs",
                allowPublicRegistration = true,
                requireEmailVerification = false,
                maxActiveSessionsPerUser = 7
            });

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, updateResponse);

            var publicResponse = await _fixture.Factory.CreateClient().GetAsync("/api/system-config");
            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, publicResponse);
            var body = await publicResponse.Content.ReadAsStringAsync();

            Assert.Contains("\"schoolName\":\"Sqeez Live Academy\"", body);
            Assert.Contains("\"supportEmail\":\"support-live@sqeez.test\"", body);
            Assert.Contains("\"defaultLanguage\":\"cs\"", body);
            Assert.Contains("\"allowPublicRegistration\":true", body);
            Assert.Contains("\"requireEmailVerification\":false", body);
            Assert.Contains("\"maxActiveSessionsPerUser\":7", body);
        }

        [DockerAvailableFact]
        public async Task UpdateConfig_AsStudent_ReturnsForbiddenAndDoesNotPersist()
        {
            await _fixture.ResetDatabaseAsync();
            var student = await AddStudentAsync();
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, student);

            var response = await client.PatchAsJsonAsync("/api/system-config", new
            {
                schoolName = "Forbidden update"
            });

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.Forbidden, response);

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var config = await dbContext.SystemConfigs.SingleAsync(config => config.Id == 1);

            Assert.Equal("Sqeez", config.SchoolName);
        }

        [DockerAvailableFact]
        public async Task CreateBadge_AsAdminMultipart_PersistsBadgeAndRules()
        {
            await _fixture.ResetDatabaseAsync();
            var admin = await AddAdminAsync();
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, admin);
            using var content = new MultipartFormDataContent
            {
                { new StringContent("Live badge"), "Name" },
                { new StringContent("Created through multipart form data."), "Description" },
                { new StringContent("25"), "XpBonus" },
                { new StringContent(nameof(BadgeMetric.TotalScore)), "Rules[0].Metric" },
                { new StringContent(nameof(BadgeOperator.GreaterThanOrEqual)), "Rules[0].Operator" },
                { new StringContent("5"), "Rules[0].TargetValue" }
            };

            var response = await client.PostAsync("/api/badges", content);

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, response);

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var badge = await dbContext.Badges.Include(badge => badge.Rules).SingleAsync();
            var rule = Assert.Single(badge.Rules);

            Assert.Equal("Live badge", badge.Name);
            Assert.Equal(25, badge.XpBonus);
            Assert.Equal(BadgeMetric.TotalScore, rule.Metric);
            Assert.Equal(BadgeOperator.GreaterThanOrEqual, rule.Operator);
            Assert.Equal(5, rule.TargetValue);
        }

        [DockerAvailableFact]
        public async Task AwardBadge_AsAdmin_PersistsStudentBadgeAndAddsXpBonus()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedBadgeAndUsersAsync();
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Admin);

            var response = await client.PostAsync($"/api/badges/{seed.Badge.Id}/award/{seed.Student.Id}", null);

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, response);

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var student = await dbContext.Students.SingleAsync(student => student.Id == seed.Student.Id);

            Assert.Equal(10, student.CurrentXP);
            Assert.True(await dbContext.StudentBadges.AnyAsync(studentBadge =>
                studentBadge.StudentId == seed.Student.Id &&
                studentBadge.BadgeId == seed.Badge.Id));
        }

        [DockerAvailableFact]
        public async Task AwardBadge_WhenAlreadyAwarded_ReturnsConflictAndKeepsSingleJoinRow()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedBadgeAndUsersAsync(awardBadge: true);
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Admin);

            var response = await client.PostAsync($"/api/badges/{seed.Badge.Id}/award/{seed.Student.Id}", null);

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.Conflict, response);

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();

            Assert.Equal(1, await dbContext.StudentBadges.CountAsync(studentBadge =>
                studentBadge.StudentId == seed.Student.Id &&
                studentBadge.BadgeId == seed.Badge.Id));
        }

        [DockerAvailableFact]
        public async Task DeleteBadge_WhenAwarded_DeletesBadgeAndAwardJoinRows()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedBadgeAndUsersAsync(awardBadge: true);
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Admin);

            var response = await client.DeleteAsync($"/api/badges/{seed.Badge.Id}");

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, response);

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();

            Assert.False(await dbContext.Badges.AnyAsync(badge => badge.Id == seed.Badge.Id));
            Assert.False(await dbContext.StudentBadges.AnyAsync(studentBadge =>
                studentBadge.StudentId == seed.Student.Id &&
                studentBadge.BadgeId == seed.Badge.Id));
        }

        [DockerAvailableFact]
        public async Task GetBadges_WithEarnedFilter_ReturnsOnlyStudentEarnedBadges()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedBadgeAndUsersAsync(awardBadge: true);
            await AddBadgeAsync("Unearned badge");
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Student);

            var response = await client.GetAsync($"/api/badges?studentId={seed.Student.Id}&isEarned=true&pageSize=10");

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, response);
            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var data = json.RootElement.GetProperty("data");

            Assert.Equal(1, data.GetArrayLength());
            Assert.Equal(seed.Badge.Id, data[0].GetProperty("id").GetInt64());
        }

        private async Task<Admin> AddAdminAsync()
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();

            return await PostgresTestHelpers.SeedAdminAsync(dbContext);
        }

        private async Task<Student> AddStudentAsync()
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();

            return await PostgresTestHelpers.SeedStudentAsync(dbContext);
        }

        private async Task<Badge> AddBadgeAsync(string name)
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var badge = new Badge
            {
                Name = name,
                Description = "Seeded badge",
                XpBonus = 10
            };

            dbContext.Badges.Add(badge);
            await dbContext.SaveChangesAsync();
            return badge;
        }

        private async Task<BadgeSeed> SeedBadgeAndUsersAsync(bool awardBadge = false)
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var admin = await PostgresTestHelpers.SeedAdminAsync(dbContext);
            var student = await PostgresTestHelpers.SeedStudentAsync(dbContext);
            var badge = new Badge
            {
                Name = "Seeded live badge",
                Description = "Seeded for live PostgreSQL integration tests.",
                XpBonus = 10
            };

            dbContext.Badges.Add(badge);
            await dbContext.SaveChangesAsync();

            if (awardBadge)
            {
                dbContext.StudentBadges.Add(new StudentBadge
                {
                    StudentId = student.Id,
                    BadgeId = badge.Id,
                    EarnedAt = DateTime.UtcNow
                });
                student.CurrentXP += badge.XpBonus;
                await dbContext.SaveChangesAsync();
            }

            return new BadgeSeed(admin, student, badge);
        }

        private sealed record BadgeSeed(Admin Admin, Student Student, Badge Badge);
    }
}
