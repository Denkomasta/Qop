using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Services;

namespace Sqeez.Api.Tests.Services
{
    public class AdminServiceTests
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

        [Fact]
        public async Task PatchAdminAsync_WhenAdminDoesNotExist_ReturnsFailure()
        {
            var context = await GetInMemoryDbContext();

            var mockLogger = new Mock<ILogger<AdminService>>();

            var service = new AdminService(context, mockLogger.Object);

            var patchDto = new PatchAdminDto { Username = "NewName" };

            var result = await service.PatchAdminAsync(999, patchDto); // 999 doesn't exist

            Assert.NotNull(result);
            Assert.Equal("Admin not found.", result.ErrorMessage);
        }
    }
}