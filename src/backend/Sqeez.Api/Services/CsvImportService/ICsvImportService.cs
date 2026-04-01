using Sqeez.Api.DTOs;
using Sqeez.Api.Models.Import;

namespace Sqeez.Api.Services.Interfaces
{
    public interface ICsvImportService
    {
        Task<ServiceResult<ImportResultDto>> ImportMasterFileAsync(IFormFile file);
    }
}