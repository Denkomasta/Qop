using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string subDirectory = "media")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.");

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", subDirectory);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{subDirectory}/{uniqueFileName}";
        }

        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                var relativePath = fileUrl.TrimStart('/');
                var physicalPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relativePath);

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting physical file at {FileUrl}", fileUrl);
                return Task.FromResult(false);
            }
        }
    }
}