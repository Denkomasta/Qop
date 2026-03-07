using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;
using BC = BCrypt.Net.BCrypt;

namespace Sqeez.Api.Services.UserService
{
    public class StudentService : BaseService<StudentService>, IStudentService
    {
        public StudentService(SqeezDbContext context, ILogger<StudentService> logger) : base(context, logger) { }

        public async Task<ServiceResult<PagedResponse<StudentDto>>> GetAllStudentsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Students.AsNoTracking();

            int totalCount = await query.CountAsync();

            var students = await query
                .OrderBy(s => s.Username)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new StudentDto
                {
                    Id = s.Id,
                    Username = s.Username,
                    Email = s.Email,
                    CurrentXP = s.CurrentXP,
                    Role = s.Role.ToString(),
                    IsOnline = s.IsOnline,
                    SchoolClassId = s.SchoolClassId
                })
                .ToListAsync();

            var response = new PagedResponse<StudentDto>
            {
                Data = students,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<PagedResponse<StudentDto>>.Ok(response);
        }

        public async Task<ServiceResult<StudentDto>> GetStudentByIdAsync(long id)
        {
            var student = await _context.Students
                .Where(s => s.Id == id && s.Role == UserRole.Student)
                .Select(s => new StudentDto
                {
                    Id = s.Id,
                    Username = s.Username,
                    Email = s.Email,
                    CurrentXP = s.CurrentXP,
                    Role = s.Role.ToString(),
                    IsOnline = s.IsOnline,
                    SchoolClassId = s.SchoolClassId
                })
                .FirstOrDefaultAsync();

            if (student == null) return ServiceResult<StudentDto>.Failure("Student not found.", ServiceError.NotFound);
            return ServiceResult<StudentDto>.Ok(student);
        }

        public async Task<ServiceResult<StudentDto>> CreateStudentAsync(CreateStudentDto dto)
        {
            if (await _context.Students.AnyAsync(u => u.Email == dto.Email.Trim().ToLower()))
                return ServiceResult<StudentDto>.Failure("Email already in use.", ServiceError.Conflict);

            var student = new Student
            {
                Username = dto.Username,
                Email = dto.Email.Trim().ToLower(),
                PasswordHash = dto.Password,    // should be already hashed!
                Role = UserRole.Student,
                LastSeen = DateTime.UtcNow,
                SchoolClassId = dto.SchoolClassId
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var resultDto = new StudentDto
            {
                Id = student.Id,
                Username = student.Username,
                Email = student.Email,
                Role = student.Role.ToString(),
                SchoolClassId = student.SchoolClassId
            };

            return ServiceResult<StudentDto>.Ok(resultDto);
        }

        public async Task<ServiceResult<bool>> UpdateStudentAsync(long id, UpdateStudentDto dto)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id && s.Role == UserRole.Student);
            if (student == null) return ServiceResult<bool>.Failure("Student not found.", ServiceError.NotFound);

            student.Username = dto.Username;
            student.Email = dto.Email.Trim().ToLower();
            student.SchoolClassId = dto.SchoolClassId;

            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> DeleteStudentAsync(long id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id && s.Role == UserRole.Student);
            if (student == null) return ServiceResult<bool>.Failure("Student not found.", ServiceError.NotFound);

            // TODO for now only archive
            student.IsArchived = true;
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }
    }
}