using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Media;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services;

namespace Sqeez.Api.Tests.Services
{
    public class MediaAssetServiceTests
    {
        private async Task<SqeezDbContext> GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SqeezDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SqeezDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return context;
        }

        private MediaAssetService CreateService(SqeezDbContext context)
        {
            var mockLogger = new Mock<ILogger<MediaAssetService>>();
            return new MediaAssetService(context, mockLogger.Object);
        }

        [Fact]
        public async Task GetMediaAssetByIdAsync_WhenExists_ReturnsDto()
        {
            var context = await GetInMemoryDbContext();

            // Setup a teacher to satisfy the Owner relationship
            var teacher = new Teacher { Username = "Mr. Snaps", Role = UserRole.Teacher };
            context.Teachers.Add(teacher);

            var asset = new MediaAsset
            {
                LocationUrl = "https://s3.amazonaws.com/image.png",
                MimeType = MediaType.Image,
                IsPrivate = false,
                Owner = teacher
            };
            context.MediaAssets.Add(asset);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetMediaAssetByIdAsync(asset.Id);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("https://s3.amazonaws.com/image.png", result.Data.LocationUrl);
            Assert.Equal("Mr. Snaps", result.Data.OwnerUsername);
        }

        [Fact]
        public async Task GetAllMediaAssetsAsync_WithOwnerFilter_ReturnsFilteredResults()
        {
            var context = await GetInMemoryDbContext();

            var teacher1 = new Teacher { Username = "Teacher1", Role = UserRole.Teacher };
            var teacher2 = new Teacher { Username = "Teacher2", Role = UserRole.Teacher };
            context.Teachers.AddRange(teacher1, teacher2);

            context.MediaAssets.AddRange(
                new MediaAsset { LocationUrl = "url1", Owner = teacher1 },
                new MediaAsset { LocationUrl = "url2", Owner = teacher1 },
                new MediaAsset { LocationUrl = "url3", Owner = teacher2 }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var filter = new MediaAssetFilterDto { OwnerId = teacher1.Id };
            var result = await service.GetAllMediaAssetsAsync(filter);

            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.TotalCount);
            Assert.All(result.Data.Data, a => Assert.Equal(teacher1.Id, a.OwnerId));
        }

        [Fact]
        public async Task CreateMediaAssetAsync_WhenValidOwner_CreatesAsset()
        {
            var context = await GetInMemoryDbContext();
            var teacher = new Teacher { Username = "ValidOwner", Role = UserRole.Teacher };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new CreateMediaAssetDto(
                "https://domain.com/video.mp4",
                MediaType.Video,
                true,
                teacher.Id,
                "A private video"
            );

            var result = await service.CreateMediaAssetAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("https://domain.com/video.mp4", result.Data!.LocationUrl);
            Assert.Equal("ValidOwner", result.Data.OwnerUsername);

            var dbAsset = await context.MediaAssets.FindAsync(result.Data.Id);
            Assert.NotNull(dbAsset);
            Assert.Equal(teacher.Id, dbAsset.OwnerId);
        }

        [Fact]
        public async Task CreateMediaAssetAsync_WhenInvalidOwner_ReturnsNotFound()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);

            // 999 is a Teacher ID that does not exist in the in-memory database
            var dto = new CreateMediaAssetDto("url", (MediaType)1, false, 999);

            var result = await service.CreateMediaAssetAsync(dto);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
            Assert.Contains("does not exist", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteMediaAssetAsync_WhenExists_DeletesAsset()
        {
            var context = await GetInMemoryDbContext();
            var teacher = new Teacher { Username = "Owner", Role = UserRole.Teacher };
            var asset = new MediaAsset { LocationUrl = "url", Owner = teacher };

            context.Teachers.Add(teacher);
            context.MediaAssets.Add(asset);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteMediaAssetAsync(asset.Id);

            Assert.True(result.Success);
            var dbAsset = await context.MediaAssets.FindAsync(asset.Id);
            Assert.Null(dbAsset);
        }
    }
}