using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Enums;
using Sqeez.Api.Services;
using System.Text;
using Xunit;

namespace Sqeez.Api.Tests.Services
{
    public class LocalFileStorageServiceTests : IDisposable
    {
        private readonly string _tempWebRootPath;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<ILogger<LocalFileStorageService>> _mockLogger;
        private readonly LocalFileStorageService _service;

        public LocalFileStorageServiceTests()
        {
            _tempWebRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempWebRootPath);

            _mockEnv = new Mock<IWebHostEnvironment>();
            // Mock both so it works regardless of whether you use wwwroot or SecureStorage!
            _mockEnv.Setup(e => e.WebRootPath).Returns(_tempWebRootPath);
            _mockEnv.Setup(e => e.ContentRootPath).Returns(_tempWebRootPath);

            _mockLogger = new Mock<ILogger<LocalFileStorageService>>();

            _service = new LocalFileStorageService(_mockEnv.Object, _mockLogger.Object);
        }

        private IFormFile CreateMockFormFile(string content, string fileName)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            var file = new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };
            return file;
        }

        [Fact]
        public async Task UploadFileAsync_WhenValidFile_SavesFileAndReturnsUrl()
        {
            var mockFile = CreateMockFormFile("This is a test file.", "test-image.jpg");

            var response = await _service.UploadFileAsync(mockFile, "test-media");

            Assert.True(response.Success); // Ensure the upload succeeded
            var resultUrl = response.Data;

            Assert.NotNull(resultUrl);
            Assert.StartsWith("/uploads/test-media/", resultUrl);
            Assert.EndsWith(".jpg", resultUrl);

            var relativePath = resultUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(_tempWebRootPath, "SecureStorage", relativePath);

            Assert.True(File.Exists(physicalPath));
        }

        [Fact]
        public async Task UploadFileAsync_WhenNullFile_ReturnsFailureResult()
        {
            IFormFile nullFile = null!;

            // We no longer expect a crash, we expect a graceful failure!
            var result = await _service.UploadFileAsync(nullFile);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.ValidationFailed, result.ErrorCode);
            Assert.Equal("No file was uploaded.", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteFileAsync_WhenFileExists_DeletesFileAndReturnsTrue()
        {
            var mockFile = CreateMockFormFile("To be deleted", "delete-me.jpg");
            var response = await _service.UploadFileAsync(mockFile, "temp");
            var fileUrl = response.Data;

            Assert.NotNull(fileUrl);

            var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(_tempWebRootPath, "SecureStorage", relativePath);

            Assert.True(File.Exists(physicalPath)); // Prove it's there first

            var result = await _service.DeleteFileAsync(fileUrl);

            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.False(File.Exists(physicalPath)); // Prove it's gone
        }

        [Fact]
        public async Task DeleteFileAsync_WhenFileDoesNotExist_ReturnsTrue()
        {
            var fakeUrl = "/uploads/media/does-not-exist.jpg";

            var result = await _service.DeleteFileAsync(fakeUrl);

            // Because it's idempotent, deleting a missing file is considered a success
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempWebRootPath))
            {
                Directory.Delete(_tempWebRootPath, true);
            }
        }
    }
}