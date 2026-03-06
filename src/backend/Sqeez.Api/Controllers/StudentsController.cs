using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;

namespace Sqeez.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly SqeezDbContext _context;

        public StudentsController(SqeezDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentResponseDTO>>> GetStudents()
        {
            var students = await _context.Students
                .OrderBy(s => s.Id)
                .Select(s => new StudentResponseDTO
                {
                    Id = s.Id,
                    Username = s.Username,
                    Email = s.Email,
                    CurrentXP = s.CurrentXP,
                    Role = s.Role,
                    LastSeen = s.LastSeen
                })
                .ToListAsync();

            return Ok(students);
        }
    }
}