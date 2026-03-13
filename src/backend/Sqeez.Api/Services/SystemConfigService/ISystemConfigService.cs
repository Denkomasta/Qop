using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface ISystemConfigService
    {
        Task<ServiceResult<SystemConfigDto>> GetConfigAsync();
        Task<ServiceResult<SystemConfigDto>> UpdateConfigAsync(UpdateSystemConfigDto dto);
    }
}