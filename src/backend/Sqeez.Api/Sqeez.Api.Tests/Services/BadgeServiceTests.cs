using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Gamification;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Tests.Services
{
    public class BadgeServiceTests
    {
        private readonly DbContextOptions<SqeezDbContext> _dbContextOptions;
        private readonly Mock<ILogger<BadgeService>> _mockLogger;
        private readonly Mock<IFileStorageService> _mockFileStorage;

        public BadgeServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<SqeezDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockLogger = new Mock<ILogger<BadgeService>>();

            _mockFileStorage = new Mock<IFileStorageService>();
            _mockFileStorage.Setup(fs => fs.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(ServiceResult<string>.Ok("/badges/mock-icon.png"));
            _mockFileStorage.Setup(fs => fs.DeleteFileAsync(It.IsAny<string>()))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));
        }

        private async Task<SqeezDbContext> GetSeededContextAsync()
        {
            var context = new SqeezDbContext(_dbContextOptions);

            var student = new Student { Id = 1, Username = "gamestudent", Email = "game@test.com", PasswordHash = "hash", CurrentXP = 0 };

            var perfectBadge = new Badge
            {
                Id = 1,
                Name = "Perfect Score",
                Description = "100% on a quiz!",
                XpBonus = 100,
                Rules = new List<BadgeRule>
                {
                    new BadgeRule { Id = 1, BadgeId = 1, Metric = BadgeMetric.ScorePercentage, Operator = BadgeOperator.Equals, TargetValue = 100 }
                }
            };

            var highScorerBadge = new Badge
            {
                Id = 2,
                Name = "High Scorer",
                Description = "Scored over 50 points",
                XpBonus = 50,
                Rules = new List<BadgeRule>
                {
                    new BadgeRule { Id = 2, BadgeId = 2, Metric = BadgeMetric.TotalScore, Operator = BadgeOperator.GreaterThan, TargetValue = 50 }
                }
            };

            context.Students.Add(student);
            context.Badges.AddRange(perfectBadge, highScorerBadge);

            await context.SaveChangesAsync();
            return context;
        }

        [Fact]
        public async Task CreateBadgeAsync_WithRules_AddsBadgeAndRulesToDatabase()
        {
            await using var context = await GetSeededContextAsync();

            var service = new BadgeService(context, _mockLogger.Object, _mockFileStorage.Object);

            var rules = new List<CreateBadgeRuleDto>
            {
                new CreateBadgeRuleDto(BadgeMetric.TotalScore, BadgeOperator.GreaterThanOrEqual, 1000)
            };

            var dto = new CreateBadgeDto
            {
                Name = "1K Club",
                Description = "Score 1000 points total",
                IconFile = null, // Testing without an image upload
                XpBonus = 200,
                Rules = rules
            };

            var result = await service.CreateBadgeAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("1K Club", result.Data!.Name);
            Assert.Single(result.Data.Rules);

            Assert.Equal(3, await context.Badges.CountAsync());
            Assert.Equal(3, await context.BadgeRules.CountAsync());
        }

        [Fact]
        public async Task UpdateBadgeAsync_WithUpsertRules_SyncsDatabasePerfectly()
        {
            await using var context = await GetSeededContextAsync();
            var service = new BadgeService(context, _mockLogger.Object, _mockFileStorage.Object);

            var upsertRules = new List<UpdateBadgeRuleDto>
            {
                new UpdateBadgeRuleDto(1, BadgeMetric.ScorePercentage, BadgeOperator.Equals, 90),
                new UpdateBadgeRuleDto(null, BadgeMetric.TotalScore, BadgeOperator.GreaterThanOrEqual, 500)
            };

            var dto = new UpdateBadgeDto
            {
                Name = "Updated Name",
                Rules = upsertRules
            };

            // Test execution & assertions...
            var result = await service.UpdateBadgeAsync(1, dto);

            Assert.True(result.Success);
            Assert.Equal("Updated Name", result.Data!.Name);
            Assert.Equal("100% on a quiz!", result.Data.Description);
            Assert.Equal(100, result.Data.XpBonus);

            Assert.Equal(2, result.Data.Rules.Count);

            var dbBadge = await context.Badges.Include(b => b.Rules).FirstAsync(b => b.Id == 1);
            Assert.Equal(2, dbBadge.Rules.Count);

            var updatedRule = dbBadge.Rules.First(r => r.Id == 1);
            Assert.Equal(90, updatedRule.TargetValue);

            var newRule = dbBadge.Rules.First(r => r.Id != 1);
            Assert.Equal(500, newRule.TargetValue);
        }

        [Fact]
        public async Task AwardBadgeToStudentAsync_WhenValid_AwardsBadgeAndAddsXP()
        {
            await using var context = await GetSeededContextAsync();
            var service = new BadgeService(context, _mockLogger.Object, _mockFileStorage.Object);

            var result = await service.AwardBadgeToStudentAsync(1, 1);

            Assert.True(result.Success);

            var updatedStudent = await context.Students.FindAsync(1L);
            Assert.Equal(100, updatedStudent!.CurrentXP);
        }

        [Fact]
        public async Task AwardBadgeToStudentAsync_WhenAlreadyEarned_ReturnsConflict()
        {
            await using var context = await GetSeededContextAsync();

            var student = await context.Students.FindAsync(1L);
            student!.CurrentXP = 100;
            context.StudentBadges.Add(new StudentBadge { StudentId = 1, BadgeId = 1, EarnedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var service = new BadgeService(context, _mockLogger.Object, _mockFileStorage.Object);

            var result = await service.AwardBadgeToStudentAsync(1, 1);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.Conflict, result.ErrorCode);

            // xp should not increase
            var checkedStudent = await context.Students.FindAsync(1L);
            Assert.Equal(100, checkedStudent!.CurrentXP);
        }

        [Fact]
        public async Task EvaluateAndAwardBadgesAsync_WhenMeetsRules_AwardsBadge()
        {
            await using var context = await GetSeededContextAsync();
            var service = new BadgeService(context, _mockLogger.Object, _mockFileStorage.Object);

            // Student scores exactly 100% and 60 Total Points
            var metrics = new BadgeEvaluationMetrics(ScorePercentage: 100m, TotalScore: 60, 0, 0);

            await service.EvaluateAndAwardBadgesAsync(1, metrics);

            var earnedBadges = await context.StudentBadges.Where(sb => sb.StudentId == 1).ToListAsync();
            Assert.Equal(2, earnedBadges.Count);

            // Student gets 100 XP (Badge 1) + 50 XP (Badge 2) = 150 XP total
            var student = await context.Students.FindAsync(1L);
            Assert.Equal(150, student!.CurrentXP);
        }

        [Fact]
        public async Task EvaluateAndAwardBadgesAsync_WhenFailsRules_DoesNotAwardBadge()
        {
            await using var context = await GetSeededContextAsync();
            var service = new BadgeService(context, _mockLogger.Object, _mockFileStorage.Object);

            // Student scores 90% and 40 Total Points (Fails both conditions)
            var metrics = new BadgeEvaluationMetrics(ScorePercentage: 90m, TotalScore: 40, 0, 0);

            await service.EvaluateAndAwardBadgesAsync(1, metrics);

            // No badges awarded
            var earnedBadges = await context.StudentBadges.Where(sb => sb.StudentId == 1).ToListAsync();
            Assert.Empty(earnedBadges);
        }

        [Fact]
        public async Task EvaluateAndAwardBadgesAsync_WhenAlreadyEarned_SkipsEvaluation()
        {
            await using var context = await GetSeededContextAsync();

            // Give the student Badge 1 manually
            context.StudentBadges.Add(new StudentBadge { StudentId = 1, BadgeId = 1, EarnedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var service = new BadgeService(context, _mockLogger.Object, _mockFileStorage.Object);

            // Metrics perfect for Badge 1, but they already have it!
            var metrics = new BadgeEvaluationMetrics(ScorePercentage: 100m, TotalScore: 0, 0, 0);

            await service.EvaluateAndAwardBadgesAsync(1, metrics);

            // Still only has 1 badge, didn't award a duplicate
            var earnedBadges = await context.StudentBadges.Where(sb => sb.StudentId == 1).ToListAsync();
            Assert.Single(earnedBadges);
        }
    }
}