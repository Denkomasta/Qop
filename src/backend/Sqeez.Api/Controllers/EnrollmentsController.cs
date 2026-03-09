using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqeez.Api.DTOs;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Controllers
{
    [Authorize]
    [Route("api/enrollments")]
    public class EnrollmentsController : ApiBaseController
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        /// <summary>
        /// GET /api/enrollments
        /// Global search for enrollments (useful for Admins or global gradebooks)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAllEnrollments([FromQuery] EnrollmentFilterDto filter)
        {
            var result = await _enrollmentService.GetAllEnrollmentsAsync(filter);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// GET /api/enrollments/452
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetEnrollmentById(long id)
        {
            var result = await _enrollmentService.GetEnrollmentByIdAsync(id);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// PATCH /api/enrollments/452
        /// Used by teachers to grade a student (update the Mark)
        /// </summary>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPatch("{id}")]
        public async Task<ActionResult> PatchEnrollment(long id, [FromBody] PatchEnrollmentDto dto)
        {
            var result = await _enrollmentService.PatchEnrollmentAsync(id, dto);
            return HandleServiceResult(result);
        }

        /// <summary>
        /// DELETE /api/enrollments/452
        /// Used by Admins to delete a specific enrollment record directly
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEnrollment(long id)
        {
            var result = await _enrollmentService.DeleteEnrollmentAsync(id);
            return HandleServiceResult(result);
        }
    }
}