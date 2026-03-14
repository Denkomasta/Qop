using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ISystemConfigService _configService;
        private readonly ILogger<LocalFileStorageService> _logger;
        private const long MaxFileSize = 5 * 1024 * 1024;   // Currently 5 MB, TODO change to modular solution
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mp3", ".pdf" };

        public LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger, ISystemConfigService configService)
        {
            _env = env;
            _logger = logger;
            _configService = configService;
        }

        public async Task<ServiceResult<string>> UploadFileAsync(IFormFile file, string subDirectory = "media")
        {
            if (file == null || file.Length == 0)
                return ServiceResult<string>.Failure("No file was uploaded.", ServiceError.ValidationFailed);

            var config = await _configService.GetConfigAsync();

            long maxSizeBytes = config.Data!.MaxFileUploadSizeMB * 1024 * 1024;

            if (file.Length > maxSizeBytes)
            {
                return ServiceResult<string>.Failure(
                    $"File is too large. Maximum allowed size is {config.Data.MaxFileUploadSizeMB} MB.",
                    ServiceError.ValidationFailed);
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
            {
                var allowed = string.Join(", ", _allowedExtensions);
                return ServiceResult<string>.Failure($"Invalid file type '{extension}'. Allowed types are: {allowed}", ServiceError.ValidationFailed);
            }

            try
            {
                var rootPath = Path.Combine(_env.ContentRootPath ?? Directory.GetCurrentDirectory(), "SecureStorage");
                var uploadsFolder = Path.Combine(rootPath, "uploads", subDirectory);
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return ServiceResult<string>.Ok($"/uploads/{subDirectory}/{uniqueFileName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving physical file.");
                return ServiceResult<string>.Failure("An unexpected error occurred while saving the file.", ServiceError.InternalError);
            }
        }

        public Task<ServiceResult<string>> GetPhysicalFilePathAsync(string fileUrl)
        {
            // Task.FromResult to satisfy the Task method signature
            if (string.IsNullOrWhiteSpace(fileUrl) || fileUrl.Contains(".."))
            {
                return Task.FromResult(ServiceResult<string>.Failure("Invalid file path.", ServiceError.ValidationFailed));
            }

            var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var rootPath = Path.Combine(_env.ContentRootPath ?? Directory.GetCurrentDirectory(), "SecureStorage");
            var physicalPath = Path.Combine(rootPath, relativePath);

            var fullRootPath = Path.GetFullPath(rootPath);
            var fullPhysicalPath = Path.GetFullPath(physicalPath);

            if (!fullPhysicalPath.StartsWith(fullRootPath))
            {
                return Task.FromResult(ServiceResult<string>.Failure("Access denied.", ServiceError.Forbidden));
            }

            if (!File.Exists(fullPhysicalPath))
            {
                return Task.FromResult(ServiceResult<string>.Failure("The physical file is missing from the server.", ServiceError.NotFound));
            }

            return Task.FromResult(ServiceResult<string>.Ok(fullPhysicalPath));
        }

        public Task<ServiceResult<bool>> DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileUrl) || fileUrl.Contains(".."))
                {
                    _logger.LogWarning("Blocked attempt to delete invalid or unsafe file path: {FileUrl}", fileUrl);
                    return Task.FromResult(ServiceResult<bool>.Failure("Invalid file path.", ServiceError.ValidationFailed));
                }

                var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var rootPath = Path.Combine(_env.ContentRootPath ?? Directory.GetCurrentDirectory(), "SecureStorage");
                var physicalPath = Path.Combine(rootPath, relativePath);

                var fullRootPath = Path.GetFullPath(rootPath);
                var fullPhysicalPath = Path.GetFullPath(physicalPath);

                if (!fullPhysicalPath.StartsWith(fullRootPath))
                {
                    _logger.LogWarning("Blocked attempt to delete file outside of WebRoot: {FileUrl}", fileUrl);
                    return Task.FromResult(ServiceResult<bool>.Failure("Access denied.", ServiceError.Forbidden));
                }

                if (File.Exists(fullPhysicalPath))
                {
                    File.Delete(fullPhysicalPath);
                    return Task.FromResult(ServiceResult<bool>.Ok(true));
                }

                return Task.FromResult(ServiceResult<bool>.Ok(true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting physical file at {FileUrl}", fileUrl);
                return Task.FromResult(ServiceResult<bool>.Failure("An unexpected error occurred while deleting the file.", ServiceError.InternalError));
            }
        }
    }
}