using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Controllers
{
    [Authorize]
    [Route("api/media-assets")]
    public class MediaAssetsController : ApiBaseController
    {
        private readonly IMediaAssetService _mediaAssetService;
        private readonly IFileStorageService _fileStorageService;

        public MediaAssetsController(IMediaAssetService mediaAssetService, IFileStorageService fileStorageService)
        {
            _mediaAssetService = mediaAssetService;
            _fileStorageService = fileStorageService;
        }

        /// <summary>
        /// GET /api/media-assets
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<MediaAssetDto>>> GetAll([FromQuery] MediaAssetFilterDto filter)
        {
            var result = await _mediaAssetService.GetAllMediaAssetsAsync(filter);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// GET /api/media-assets/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MediaAssetDto>> GetById(long id)
        {
            var result = await _mediaAssetService.GetMediaAssetByIdAsync(id);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// POST /api/media-assets
        /// Saves metadata for a new media asset (without a file upload).
        /// </summary>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost]
        public async Task<ActionResult<MediaAssetDto>> Create([FromBody] CreateMediaAssetDto dto)
        {
            var result = await _mediaAssetService.CreateMediaAssetAsync(dto);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// PATCH /api/media-assets/{id}
        /// </summary>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPatch("{id}")]
        public async Task<ActionResult<MediaAssetDto>> Patch(long id, [FromBody] PatchMediaAssetDto dto)
        {
            var result = await _mediaAssetService.PatchMediaAssetAsync(id, dto);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// POST /api/media-assets/upload
        /// Accepts a physical file, saves it securely, and creates the database record.
        /// </summary>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<MediaAssetDto>> UploadFile([FromForm] UploadMediaAssetDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("No file was uploaded.");

            var userIdStr = GetUserIdFromClaims();
            if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long ownerId))
            {
                return Unauthorized();
            }

            try
            {
                var mimeTypeStr = dto.File.ContentType.ToLower();
                MediaType mediaType = MediaType.Document;
                if (mimeTypeStr.StartsWith("image/")) mediaType = MediaType.Image;
                else if (mimeTypeStr.StartsWith("video/")) mediaType = MediaType.Video;
                else if (mimeTypeStr.StartsWith("audio/")) mediaType = MediaType.Audio;

                var response = await _fileStorageService.UploadFileAsync(dto.File, "media", false);

                if (!response.Success)
                {
                    return HandleServiceResult(response);
                }

                string fileUrl = response.Data!;

                var createDto = new CreateMediaAssetDto(
                    LocationUrl: fileUrl,
                    MimeType: mediaType,
                    IsPrivate: dto.IsPrivate,
                    OwnerId: ownerId,
                    Description: dto.Description
                );

                var dbResult = await _mediaAssetService.CreateMediaAssetAsync(createDto);

                if (!dbResult.Success)
                {
                    await _fileStorageService.DeleteFileAsync(fileUrl);
                    return HandleServiceResult(dbResult);
                }

                return HandleServiceResult(dbResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred during file upload: {ex.Message}");
            }
        }

        /// <summary>
        /// DELETE /api/media-assets/{id}
        /// Deletes the database metadata AND the physical file from the server.
        /// </summary>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var dbDeleteResult = await _mediaAssetService.DeleteMediaAssetAsync(id);
            return HandleServiceResult(dbDeleteResult);
        }

        /// <summary>
        /// GET /api/media-assets/{id}/file
        /// Streams a secure file back to authorized users.
        /// </summary>
        [Authorize]
        [HttpGet("{id}/file")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK, "application/octet-stream")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFile(long id)
        {
            var userIdStr = GetUserIdFromClaims();
            var role = GetUserRoleFromClaims() ?? string.Empty;
            long.TryParse(userIdStr, out long currentUserId);

            var metadataResult = await _mediaAssetService.GetDownloadMetadataAsync(id, currentUserId, role);
            if (!metadataResult.Success)
            {
                return HandleServiceResult(metadataResult);
            }

            var pathResult = await _fileStorageService.GetPhysicalFilePathAsync(metadataResult.Data!.LocationUrl);
            if (!pathResult.Success)
            {
                return HandleServiceResult(pathResult);
            }

            var fileName = Path.GetFileName(metadataResult.Data!.LocationUrl);

            return PhysicalFile(pathResult.Data!, metadataResult.Data.MimeType, fileName, enableRangeProcessing: true);
        }
    }
}