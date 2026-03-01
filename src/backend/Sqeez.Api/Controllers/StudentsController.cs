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

        [HttpPost]
        public async Task<ActionResult<StudentResponseDTO>> CreateStudent(StudentCreateDTO dto)
        {
            var newStudent = new Student
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = dto.Password,    // TODO: Hash the password before saving
                Role = UserRole.Student,
                LastSeen = DateTime.UtcNow,
                IsOnline = true,
                IsArchived = false
            };

            _context.Students.Add(newStudent);
            await _context.SaveChangesAsync();

            var responseDto = new StudentResponseDTO
            {
                Id = newStudent.Id,
                Username = newStudent.Username,
                Email = newStudent.Email,
                CurrentXP = newStudent.CurrentXP,
                Role = newStudent.Role,
                LastSeen = newStudent.LastSeen
            };

            return CreatedAtAction(nameof(GetStudents), new { id = newStudent.Id }, responseDto);
        }
    }
}