using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Gamification;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services;
using Xunit;

namespace Sqeez.Api.Tests.Services
{
    public class BadgeServiceTests
    {
        private readonly DbContextOptions<SqeezDbContext> _dbContextOptions;
        private readonly Mock<ILogger<BadgeService>> _mockLogger;

        public BadgeServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<SqeezDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockLogger = new Mock<ILogger<BadgeService>>();
        }

        private async Task<SqeezDbContext> GetSeededContextAsync()
        {
            var context = new SqeezDbContext(_dbContextOptions);

            // Create a student starting with 0 XP
            var student = new Student
            {
                Id = 1,
                Username = "gamestudent",
                Email = "game@test.com",
                PasswordHash = "hash",
                CurrentXP = 0
            };

            // Create a generic badge worth 50 XP
            var badge1 = new Badge
            {
                Id = 1,
                Name = "First Quiz",
                Description = "Took your first quiz!",
                XpBonus = 50,
                Condition = "FIRST_QUIZ"
            };

            var badge2 = new Badge
            {
                Id = 2,
                Name = "Perfect Score",
                Description = "100% on a quiz!",
                XpBonus = 100,
                Condition = "PERFECT_SCORE"
            };

            context.Students.Add(student);
            context.Badges.AddRange(badge1, badge2);

            await context.SaveChangesAsync();
            return context;
        }

        [Fact]
        public async Task CreateBadgeAsync_AddsBadgeToDatabase()
        {
            await using var context = await GetSeededContextAsync();
            var service = new BadgeService(context, _mockLogger.Object);

            var dto = new CreateBadgeDto("Speed Demon", "Fastest answer", "url.png", 25, "SPEED");

            var result = await service.CreateBadgeAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("Speed Demon", result.Data!.Name);
            Assert.Equal(3, await context.Badges.CountAsync()); // We seeded 2, so this makes 3!
        }

        [Fact]
        public async Task AwardBadgeToStudentAsync_WhenValid_AwardsBadgeAndAddsXP()
        {
            await using var context = await GetSeededContextAsync();
            var service = new BadgeService(context, _mockLogger.Object);

            var result = await service.AwardBadgeToStudentAsync(1, 1);

            Assert.True(result.Success);

            var earnedBadge = await context.StudentBadges.FirstOrDefaultAsync(sb => sb.StudentId == 1 && sb.BadgeId == 1);
            Assert.NotNull(earnedBadge);

            var updatedStudent = await context.Students.FindAsync(1L);
            Assert.Equal(50, updatedStudent!.CurrentXP);
        }

        [Fact]
        public async Task AwardBadgeToStudentAsync_WhenAlreadyEarned_ReturnsConflictAndDoesNotAddXP()
        {
            await using var context = await GetSeededContextAsync();

            var student = await context.Students.FindAsync(1L);
            student!.CurrentXP = 50;
            context.StudentBadges.Add(new StudentBadge { StudentId = 1, BadgeId = 1, EarnedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var service = new BadgeService(context, _mockLogger.Object);

            var result = await service.AwardBadgeToStudentAsync(1, 1);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.Conflict, result.ErrorCode);

            var checkedStudent = await context.Students.FindAsync(1L);
            Assert.Equal(50, checkedStudent!.CurrentXP);

            Assert.Equal(1, await context.StudentBadges.CountAsync());
        }

        [Fact]
        public async Task GetStudentBadgesAsync_ReturnsCorrectBadgesInOrder()
        {
            await using var context = await GetSeededContextAsync();

            // Give the student both badges at different times
            context.StudentBadges.Add(new StudentBadge { StudentId = 1, BadgeId = 1, EarnedAt = DateTime.UtcNow.AddMinutes(-10) });
            context.StudentBadges.Add(new StudentBadge { StudentId = 1, BadgeId = 2, EarnedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var service = new BadgeService(context, _mockLogger.Object);

            var result = await service.GetStudentBadgesAsync(1);

            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count());

            Assert.Equal(2, result.Data!.First().BadgeId);
            Assert.Equal("Perfect Score", result.Data!.First().Name);
        }
    }
}