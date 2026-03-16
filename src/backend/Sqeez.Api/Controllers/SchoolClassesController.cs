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
        public async Task<ActionResult<PagedResponse<SchoolClassDto>>> GetAll([FromQuery] SchoolClassFilterDto filter)
        {
            var result = await _schoolClassService.GetAllClassesAsync(filter);
            return HandleServiceResult(result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<SchoolClassDetailDto>> GetById(long id)
        {
            var result = await _schoolClassService.GetClassByIdAsync(id);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<SchoolClassDto>> Create([FromBody] CreateSchoolClassDto dto)
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
        public async Task<ActionResult<SchoolClassDto>> Patch(long id, [FromBody] PatchSchoolClassDto dto)
        {
            var result = await _schoolClassService.PatchClassAsync(id, dto);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(long id)
        {
            var result = await _schoolClassService.DeleteClassAsync(id);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/students")]
        public async Task<ActionResult<bool>> AssignStudents(long id, [FromBody] AssignStudentsDto dto)
        {
            var result = await _schoolClassService.AssignStudentsToClassAsync(id, dto);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/students/remove")]
        public async Task<ActionResult<bool>> RemoveStudents(long id, [FromBody] RemoveStudentsDto dto)
        {
            var result = await _schoolClassService.RemoveStudentsFromClassAsync(id, dto);
            return HandleServiceResult(result);
        }
    }
}