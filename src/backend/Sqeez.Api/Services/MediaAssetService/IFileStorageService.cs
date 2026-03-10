using Microsoft.AspNetCore.Http;
using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves the file to storage and returns the public URL.
        /// </summary>
        Task<ServiceResult<string>> UploadFileAsync(IFormFile file, string subDirectory = "media");

        /// <summary>
        /// Deletes the physical file from storage.
        /// </summary>
        Task<ServiceResult<bool>> DeleteFileAsync(string fileUrl);
        Task<ServiceResult<string>> GetPhysicalFilePathAsync(string fileUrl);
    }
}