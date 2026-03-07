using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqeez.Api.DTOs;
using Sqeez.Api.Services.UserService;

namespace Sqeez.Api.Controllers
{
    [Route("api/[controller]")]
    public class AdminsController : ApiBaseController
    {
        private readonly IAdminService _adminService;

        public AdminsController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            return HandleServiceResult(await _adminService.GetAllAdminsAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(long id)
        {
            return HandleServiceResult(await _adminService.GetAdminByIdAsync(id));
        }

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> Create([FromBody] CreateAdminDto dto)
        //{
        //    return HandleServiceResult(await _adminService.CreateAdminAsync(dto));
        //}

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Patch(long id, [FromBody] PatchAdminDto dto)
        {
            if (!IsIdLoggedUser(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = "Forbidden",
                    message = "You do not have permission to modify another administrator's profile."
                });
            }
            return HandleServiceResult(await _adminService.PatchAdminAsync(id, dto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            if (!IsIdLoggedUser(id))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    error = "Forbidden",
                    message = "You do not have permission to delete another administrator's profile."
                });
            }
            return HandleServiceResult(await _adminService.DeleteAdminAsync(id));
        }
    }
}