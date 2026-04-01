using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Models.System;
using Sqeez.Api.Services;

namespace Sqeez.Api.Tests.Services
{
    public class SystemConfigServiceTests
    {
        private readonly DbContextOptions<SqeezDbContext> _dbContextOptions;
        private readonly Mock<ILogger<SystemConfigService>> _mockLogger;
        private readonly IMemoryCache _cache;

        public SystemConfigServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<SqeezDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockLogger = new Mock<ILogger<SystemConfigService>>();

            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        private SqeezDbContext GetContext()
        {
            return new SqeezDbContext(_dbContextOptions);
        }

        [Fact]
        public async Task GetConfigAsync_WhenDbAndCacheAreEmpty_CreatesDefaultAndCachesIt()
        {
            await using var context = GetContext();
            var service = new SystemConfigService(context, _mockLogger.Object, _cache);

            var result = await service.GetConfigAsync();

            Assert.True(result.Success);
            Assert.Equal("Sqeez", result.Data!.SchoolName);

            Assert.Equal(1, await context.Set<SystemConfig>().CountAsync());

            Assert.True(_cache.TryGetValue("GlobalSystemConfig", out SystemConfigDto? cachedConfig));
            Assert.NotNull(cachedConfig);
            Assert.Equal("Sqeez", cachedConfig!.SchoolName);
        }

        [Fact]
        public async Task GetConfigAsync_WhenDbHasConfig_ReturnsFromDbAndCachesIt()
        {
            await using var context = GetContext();

            context.Add(new SystemConfig { Id = 1, SchoolName = "Test High School", MaxAvatarAndBadgeUploadSizeMB = 99, MaxQuizMediaUploadSizeMB = 100 });
            await context.SaveChangesAsync();

            var service = new SystemConfigService(context, _mockLogger.Object, _cache);

            var result = await service.GetConfigAsync();

            Assert.True(result.Success);
            Assert.Equal("Test High School", result.Data!.SchoolName);
            Assert.Equal(99, result.Data.MaxAvatarAndBadgeUploadSizeMB);
            Assert.Equal(100, result.Data.MaxQuizMediaUploadSizeMB);

            Assert.True(_cache.TryGetValue("GlobalSystemConfig", out SystemConfigDto? cachedConfig));
            Assert.Equal(99, cachedConfig!.MaxAvatarAndBadgeUploadSizeMB);
            Assert.Equal(100, cachedConfig!.MaxQuizMediaUploadSizeMB);
        }

        [Fact]
        public async Task GetConfigAsync_WhenCacheIsPopulated_ReturnsFromCacheWithoutHittingDb()
        {
            await using var context = GetContext();
            var service = new SystemConfigService(context, _mockLogger.Object, _cache);

            var fakeCachedDto = new SystemConfigDto("Cached School", "", "", "en", "24/25", true, true, 5, 10, 1);
            _cache.Set("GlobalSystemConfig", fakeCachedDto);

            var result = await service.GetConfigAsync();

            Assert.True(result.Success);
            Assert.Equal("Cached School", result.Data!.SchoolName);

            Assert.Equal(0, await context.Set<SystemConfig>().CountAsync());
        }

        [Fact]
        public async Task UpdateConfigAsync_WhenPartialPatch_UpdatesOnlyProvidedFieldsAndRefreshesCache()
        {
            await using var context = GetContext();

            context.Add(new SystemConfig { Id = 1, SchoolName = "Old School", MaxAvatarAndBadgeUploadSizeMB = 10, MaxQuizMediaUploadSizeMB = 20 });
            await context.SaveChangesAsync();

            var service = new SystemConfigService(context, _mockLogger.Object, _cache);

            var dto = new UpdateSystemConfigDto(
                SchoolName: null, LogoUrl: null, SupportEmail: null, DefaultLanguage: null, CurrentAcademicYear: null,
                AllowPublicRegistration: null, RequireEmailVerification: null,
                MaxAvatarAndBadgeUploadSizeMB: 50, MaxQuizMediaUploadSizeMB: 60, MaxActiveSessionsPerUser: null
            );

            var result = await service.UpdateConfigAsync(dto);

            Assert.True(result.Success);
            Assert.Equal(50, result.Data!.MaxAvatarAndBadgeUploadSizeMB);
            Assert.Equal(60, result.Data!.MaxQuizMediaUploadSizeMB);

            Assert.Equal("Old School", result.Data.SchoolName);

            _cache.TryGetValue("GlobalSystemConfig", out SystemConfigDto? cachedConfig);
            Assert.Equal(50, cachedConfig!.MaxAvatarAndBadgeUploadSizeMB);
            Assert.Equal(60, cachedConfig!.MaxQuizMediaUploadSizeMB);
        }
    }
}