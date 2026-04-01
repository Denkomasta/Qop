using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqeez.Api.Models.Import;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Controllers
{
    [Authorize]
    [Route("api/import")]
    public class ImportController : ApiBaseController
    {
        private readonly ICsvImportService _csvImportService;

        public ImportController(ICsvImportService csvImportService)
        {
            _csvImportService = csvImportService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("master")]
        public async Task<ActionResult<ImportResultDto>> ImportMasterFile(IFormFile file)
        {
            var result = await _csvImportService.ImportMasterFileAsync(file);
            return HandleServiceResult(result);
        }
    }
}