using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqeez.Api.DTOs;
using Sqeez.Api.Services.UserService;

namespace Sqeez.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class TeachersController : ApiBaseController
    {
        private readonly ITeacherService _teacherService;

        public TeachersController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] TeacherFilterDto filter)
        {
            return HandleServiceResult(await _teacherService.GetAllTeachersAsync(filter));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(long id)
        {
            return HandleServiceResult(await _teacherService.GetTeacherByIdAsync(id));
        }

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
        //{
        //    return HandleServiceResult(await _teacherService.CreateTeacherAsync(dto));
        //}

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Patch(long id, [FromBody] PatchTeacherDto dto)
        {
            var role = GetUserRoleFromClaims();
            if (role == "Teacher" && !IsIdLoggedUser(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = "Forbidden",
                    message = "You do not have permission to modify teacher's profile."
                });
            }
            return HandleServiceResult(await _teacherService.PatchTeacherAsync(id, dto));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Delete(long id)
        {
            var role = GetUserRoleFromClaims();
            if (role == "Teacher" && !IsIdLoggedUser(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = "Forbidden",
                    message = "You do not have permission to delete teacher's profile."
                });
            }
            return HandleServiceResult(await _teacherService.DeleteTeacherAsync(id));
        }
    }
}