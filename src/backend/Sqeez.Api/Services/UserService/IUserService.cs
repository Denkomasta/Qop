using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult<string>> UploadAvatarAsync(long userId, IFormFile imageFile);
    }
}