using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Services;
using System.Text;
using Xunit;

namespace Sqeez.Api.Tests.Services
{
    // We implement IDisposable so the test class can clean up the physical files it creates
    public class LocalFileStorageServiceTests : IDisposable
    {
        private readonly string _tempWebRootPath;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<ILogger<LocalFileStorageService>> _mockLogger;
        private readonly LocalFileStorageService _service;

        public LocalFileStorageServiceTests()
        {
            // 1. Create a safe, temporary directory for the tests
            _tempWebRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempWebRootPath);

            // 2. Mock the Environment to point to our temporary folder
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.WebRootPath).Returns(_tempWebRootPath);

            _mockLogger = new Mock<ILogger<LocalFileStorageService>>();

            // 3. Initialize the service
            _service = new LocalFileStorageService(_mockEnv.Object, _mockLogger.Object);
        }

        // Helper method to create a fake uploaded file
        private IFormFile CreateMockFormFile(string content, string fileName)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            // We use the real FormFile class from AspNetCore.Http to simulate the upload
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

            var resultUrl = await _service.UploadFileAsync(mockFile, "test-media");

            Assert.NotNull(resultUrl);
            Assert.StartsWith("/uploads/test-media/", resultUrl);
            Assert.EndsWith(".jpg", resultUrl);

            var relativePath = resultUrl.TrimStart('/');
            var physicalPath = Path.Combine(_tempWebRootPath, relativePath);
            Assert.True(File.Exists(physicalPath));

            var savedContent = await File.ReadAllTextAsync(physicalPath);
            Assert.Equal("This is a test file.", savedContent);
        }

        [Fact]
        public async Task UploadFileAsync_WhenNullFile_ThrowsArgumentException()
        {
            IFormFile nullFile = null!;

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UploadFileAsync(nullFile));

            Assert.Equal("File is empty or null.", exception.Message);
        }

        [Fact]
        public async Task DeleteFileAsync_WhenFileExists_DeletesFileAndReturnsTrue()
        {
            var mockFile = CreateMockFormFile("To be deleted", "delete-me.txt");
            var fileUrl = await _service.UploadFileAsync(mockFile, "temp");

            var physicalPath = Path.Combine(_tempWebRootPath, fileUrl.TrimStart('/'));
            Assert.True(File.Exists(physicalPath)); // Prove it's there first

            var result = await _service.DeleteFileAsync(fileUrl);

            Assert.True(result);
            Assert.False(File.Exists(physicalPath)); // Prove it's gone
        }

        [Fact]
        public async Task DeleteFileAsync_WhenFileDoesNotExist_ReturnsFalse()
        {
            var fakeUrl = "/uploads/media/does-not-exist.jpg";

            var result = await _service.DeleteFileAsync(fakeUrl);

            Assert.False(result); // Should gracefully return false without crashing
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