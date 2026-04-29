using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Services;
using Sqeez.Api.Services.Interfaces;
using System.Text;

namespace Sqeez.Api.Tests.Services
{
    public class LocalFileStorageServiceTests : IDisposable
    {
        private readonly string _tempWebRootPath;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<ILogger<LocalFileStorageService>> _mockLogger;
        private readonly Mock<ISystemConfigService> _mockConfigService;
        private readonly LocalFileStorageService _service;

        public LocalFileStorageServiceTests()
        {
            _tempWebRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempWebRootPath);

            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.WebRootPath).Returns(_tempWebRootPath);
            _mockEnv.Setup(e => e.ContentRootPath).Returns(_tempWebRootPath);

            _mockLogger = new Mock<ILogger<LocalFileStorageService>>();

            _mockConfigService = new Mock<ISystemConfigService>();

            _mockConfigService.Setup(c => c.GetConfigAsync())
                .ReturnsAsync(ServiceResult<SystemConfigDto>.Ok(
                    new SystemConfigDto("Sqeez", "", "", "en", "24/25", true, true, 50, 50, 3)
                ));

            _service = new LocalFileStorageService(_mockEnv.Object, _mockLogger.Object, _mockConfigService.Object);
        }

        private IFormFile CreateMockFormFile(string fileName)
        {
            byte[] fileBytes;
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (extension == ".jpg" || extension == ".jpeg")
            {
                fileBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 };
            }
            else if (extension == ".png")
            {
                fileBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            }
            else if (extension == ".pdf")
            {
                fileBytes = Encoding.UTF8.GetBytes("%PDF-1.4\n");
            }
            else
            {
                fileBytes = Encoding.UTF8.GetBytes("Generic fallback content");
            }

            var stream = new MemoryStream(fileBytes);

            var file = new FormFile(stream, 0, stream.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };

            return file;
        }

        [Fact]
        public async Task UploadFileAsync_WhenSecureFile_SavesToSecureStorageAndReturnsSecureUrl()
        {
            var mockFile = CreateMockFormFile("test-image.jpg");

            var response = await _service.UploadFileAsync(mockFile, "test-media", isPublic: false);

            Assert.True(response.Success);
            var resultUrl = response.Data;

            Assert.NotNull(resultUrl);
            Assert.StartsWith("/secure/test-media/", resultUrl);
            Assert.EndsWith(".jpg", resultUrl);

            var relativePath = resultUrl.Replace("/secure/", "").Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(_tempWebRootPath, "SecureStorage", relativePath);

            Assert.True(File.Exists(physicalPath));
        }

        [Fact]
        public async Task UploadFileAsync_WhenPublicFile_SavesToWwwRootAndReturnsPublicUrl()
        {
            var mockFile = CreateMockFormFile("avatar.jpg");

            var response = await _service.UploadFileAsync(mockFile, "avatars", isPublic: true);

            Assert.True(response.Success);
            var resultUrl = response.Data;

            Assert.NotNull(resultUrl);

            Assert.StartsWith("/avatars/", resultUrl);
            Assert.EndsWith(".jpg", resultUrl);

            var relativePath = resultUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(_tempWebRootPath, relativePath);

            Assert.True(File.Exists(physicalPath));
        }

        [Fact]
        public async Task UploadFileAsync_WhenNullFile_ReturnsFailureResult()
        {
            IFormFile nullFile = null!;

            var result = await _service.UploadFileAsync(nullFile);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.ValidationFailed, result.ErrorCode);
            Assert.Equal("No file was uploaded.", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteFileAsync_WhenSecureFileExists_DeletesFileAndReturnsTrue()
        {
            var mockFile = CreateMockFormFile("delete-me.jpg");
            var response = await _service.UploadFileAsync(mockFile, "temp", isPublic: false);
            var fileUrl = response.Data;

            Assert.NotNull(fileUrl);

            var relativePath = fileUrl.Replace("/secure/", "").Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(_tempWebRootPath, "SecureStorage", relativePath);

            Assert.True(File.Exists(physicalPath));

            var result = await _service.DeleteFileAsync(fileUrl);

            Assert.True(result.Success);
            Assert.True(result.Data);
            Assert.False(File.Exists(physicalPath));
        }

        [Fact]
        public async Task DeleteFileAsync_WhenFileDoesNotExist_ReturnsTrue()
        {
            var fakeUrl = "/secure/media/does-not-exist.jpg";

            var result = await _service.DeleteFileAsync(fakeUrl);

            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task GetPhysicalFilePathAsync_WhenValidUrl_ReturnsCorrectlyRoutedPath()
        {
            var mockFile = CreateMockFormFile("test.jpg");
            var uploadResponse = await _service.UploadFileAsync(mockFile, "test", isPublic: false);
            var fileUrl = uploadResponse.Data!;

            var getResult = await _service.GetPhysicalFilePathAsync(fileUrl);

            Assert.True(getResult.Success);
            Assert.NotNull(getResult.Data);
            Assert.Contains("SecureStorage", getResult.Data);
            Assert.True(File.Exists(getResult.Data));
        }

        [Fact]
        public async Task GetPhysicalFilePathAsync_WhenPathTraversalAttempted_ReturnsForbidden()
        {
            string maliciousUrl = "/secure/media/../../windows/system32/cmd.exe";

            var getResult = await _service.GetPhysicalFilePathAsync(maliciousUrl);

            Assert.False(getResult.Success);
            Assert.Equal(ServiceError.ValidationFailed, getResult.ErrorCode);
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