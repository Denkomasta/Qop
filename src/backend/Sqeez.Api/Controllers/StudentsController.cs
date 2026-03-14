using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqeez.Api.DTOs;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class StudentsController : ApiBaseController
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] StudentFilterDto filter)
        {
            return HandleServiceResult(await _studentService.GetAllStudentsAsync(filter));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(long id)
        {
            return HandleServiceResult(await _studentService.GetStudentByIdAsync(id));
        }

        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
        //{
        //    return HandleServiceResult(await _studentService.CreateStudentAsync(dto));
        //}

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> Patch(long id, [FromBody] PatchStudentDto dto)
        {
            var role = GetUserRoleFromClaims();
            if (role != "Admin" && !IsIdLoggedUser(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = "Forbidden",
                    message = "You do not have permission to modify another student's profile."
                });
            }
            return HandleServiceResult(await _studentService.PatchStudentAsync(id, dto));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id)
        {
            var role = GetUserRoleFromClaims();
            if (role != "Admin" && !IsIdLoggedUser(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = "Forbidden",
                    message = "You do not have permission to modify another student's profile."
                });
            }
            return HandleServiceResult(await _studentService.DeleteStudentAsync(id));
        }
    }
}