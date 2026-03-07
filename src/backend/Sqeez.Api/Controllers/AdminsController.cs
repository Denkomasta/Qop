using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqeez.Api.DTOs;
using Sqeez.Api.Services.UserService;

namespace Sqeez.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminsController : ApiBaseController
    {
        private readonly IAdminService _adminService;

        public AdminsController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            return HandleServiceResult(await _adminService.GetAllAdminsAsync(pageNumber, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            return HandleServiceResult(await _adminService.GetAdminByIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAdminDto dto)
        {
            return HandleServiceResult(await _adminService.CreateAdminAsync(dto));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateAdminDto dto)
        {
            return HandleServiceResult(await _adminService.UpdateAdminAsync(id, dto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            return HandleServiceResult(await _adminService.DeleteAdminAsync(id));
        }
    }
}