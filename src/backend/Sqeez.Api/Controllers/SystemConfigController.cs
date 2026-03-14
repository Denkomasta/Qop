using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqeez.Api.DTOs;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Controllers
{
    [Route("api/system-config")]
    public class SystemConfigController : ApiBaseController
    {
        private readonly ISystemConfigService _configService;

        public SystemConfigController(ISystemConfigService configService)
        {
            _configService = configService;
        }

        [HttpGet]
        public async Task<ActionResult<SystemConfigDto>> GetConfig()
        {
            var result = await _configService.GetConfigAsync();
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch]
        public async Task<ActionResult<SystemConfigDto>> UpdateConfig([FromBody] UpdateSystemConfigDto dto)
        {
            var result = await _configService.UpdateConfigAsync(dto);
            return HandleServiceResult(result);
        }
    }
}