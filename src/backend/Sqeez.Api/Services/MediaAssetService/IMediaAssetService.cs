using Sqeez.Api.DTOs;

namespace Sqeez.Api.Services.Interfaces
{
    public interface IMediaAssetService
    {
        Task<ServiceResult<PagedResponse<MediaAssetDto>>> GetAllMediaAssetsAsync(MediaAssetFilterDto filter);
        Task<ServiceResult<MediaAssetDto>> GetMediaAssetByIdAsync(long id);
        Task<ServiceResult<MediaAssetDto>> CreateMediaAssetAsync(CreateMediaAssetDto dto);
        Task<ServiceResult<MediaAssetDto>> PatchMediaAssetAsync(long id, PatchMediaAssetDto dto);
        Task<ServiceResult<bool>> DeleteMediaAssetAsync(long id);
    }
}