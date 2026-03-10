using Microsoft.AspNetCore.Http;

namespace Sqeez.Api.Services.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves the file to storage and returns the public URL.
        /// </summary>
        Task<string> UploadFileAsync(IFormFile file, string subDirectory = "media");

        /// <summary>
        /// Deletes the physical file from storage.
        /// </summary>
        Task<bool> DeleteFileAsync(string fileUrl);
    }
}