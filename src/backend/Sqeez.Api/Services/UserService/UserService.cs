using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Services.UserService
{
    public class UserService : BaseService<UserService>, IUserService
    {
        private readonly IFileStorageService _fileStorageService;

        public UserService(
            SqeezDbContext context,
            ILogger<UserService> logger,
            IFileStorageService fileStorageService) : base(context, logger)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task<ServiceResult<string>> UploadAvatarAsync(long userId, IFormFile imageFile)
        {
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            if (!allowedImageExtensions.Contains(extension))
            {
                return ServiceResult<string>.Failure("Avatars must be an image file (.jpg, .png, .gif).", ServiceError.ValidationFailed);
            }

            var user = await _context.Students.FindAsync(userId);
            if (user == null)
            {
                return ServiceResult<string>.Failure("User not found.", ServiceError.NotFound);
            }

            // Delete the old avatar if it exists
            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                _logger.LogInformation("Deleting old avatar for user {UserId}: {Url}", userId, user.AvatarUrl);
                await _fileStorageService.DeleteFileAsync(user.AvatarUrl);
            }

            var uploadResult = await _fileStorageService.UploadFileAsync(imageFile, "avatars");
            if (!uploadResult.Success)
            {
                return ServiceResult<string>.Failure(uploadResult.ErrorMessage ?? "Internal error", uploadResult.ErrorCode);
            }

            user.AvatarUrl = uploadResult.Data!;
            await _context.SaveChangesAsync();

            return ServiceResult<string>.Ok(user.AvatarUrl);
        }
    }
}