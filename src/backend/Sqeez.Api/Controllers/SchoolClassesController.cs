using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqeez.Api.DTOs;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Controllers
{
    [Route("api/classes")]
    public class SchoolClassesController : ApiBaseController
    {
        private readonly ISchoolClassService _schoolClassService;

        public SchoolClassesController(ISchoolClassService schoolClassService)
        {
            _schoolClassService = schoolClassService;
        }

        // Maybe only authentication
        [Authorize(Roles = "Teacher,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SchoolClassFilterDto filter)
        {
            var result = await _schoolClassService.GetAllClassesAsync(filter);
            return HandleServiceResult(result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _schoolClassService.GetClassByIdAsync(id);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSchoolClassDto dto)
        {
            var result = await _schoolClassService.CreateClassAsync(dto);

            if (result.Success && result.Data != null)
            {
                // 201 code
                return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
            }

            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(long id, [FromBody] PatchSchoolClassDto dto)
        {
            var result = await _schoolClassService.PatchClassAsync(id, dto);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _schoolClassService.DeleteClassAsync(id);
            return HandleServiceResult(result);
        }
    }
}