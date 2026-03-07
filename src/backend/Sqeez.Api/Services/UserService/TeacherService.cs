using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services.UserService;
using BC = BCrypt.Net.BCrypt;

namespace Sqeez.Api.Services
{
    public class TeacherService : BaseService<TeacherService>, ITeacherService
    {
        public TeacherService(SqeezDbContext context, ILogger<TeacherService> logger) : base(context, logger) { }

        public async Task<ServiceResult<PagedResponse<TeacherDto>>> GetAllTeachersAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Teachers.AsNoTracking();

            int totalCount = await query.CountAsync();

            var teachers = await query
                .OrderBy(t => t.Username)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TeacherDto
                {
                    Id = t.Id,
                    Username = t.Username,
                    Email = t.Email,
                    CurrentXP = t.CurrentXP,
                    Role = t.Role.ToString(),
                    IsOnline = t.IsOnline,
                    SchoolClassId = t.SchoolClassId,
                    Department = t.Department // Teacher specific
                })
                .ToListAsync();

            var response = new PagedResponse<TeacherDto>
            {
                Data = teachers,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<PagedResponse<TeacherDto>>.Ok(response);
        }

        public async Task<ServiceResult<TeacherDto>> GetTeacherByIdAsync(long id)
        {
            var teacher = await _context.Teachers
                .Where(t => t.Id == id && t.Role == UserRole.Teacher)
                .Select(t => new TeacherDto
                {
                    Id = t.Id,
                    Username = t.Username,
                    Email = t.Email,
                    CurrentXP = t.CurrentXP,
                    Role = t.Role.ToString(),
                    IsOnline = t.IsOnline,
                    SchoolClassId = t.SchoolClassId,
                    Department = t.Department
                })
                .FirstOrDefaultAsync();

            if (teacher == null) return ServiceResult<TeacherDto>.Failure("Teacher not found.", ServiceError.NotFound);
            return ServiceResult<TeacherDto>.Ok(teacher);
        }

        public async Task<ServiceResult<TeacherDto>> CreateTeacherAsync(CreateTeacherDto dto)
        {
            if (await _context.Students.AnyAsync(u => u.Email == dto.Email.Trim().ToLower()))
                return ServiceResult<TeacherDto>.Failure("Email already in use.", ServiceError.Conflict);

            var teacher = new Teacher
            {
                Username = dto.Username,
                Email = dto.Email.Trim().ToLower(),
                PasswordHash = BC.HashPassword(dto.Password),
                Role = UserRole.Teacher,
                LastSeen = DateTime.UtcNow,
                SchoolClassId = dto.SchoolClassId,
                Department = dto.Department
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            var resultDto = new TeacherDto
            {
                Id = teacher.Id,
                Username = teacher.Username,
                Email = teacher.Email,
                Role = teacher.Role.ToString(),
                SchoolClassId = teacher.SchoolClassId,
                Department = teacher.Department
            };

            return ServiceResult<TeacherDto>.Ok(resultDto);
        }

        public async Task<ServiceResult<bool>> PatchTeacherAsync(long id, PatchTeacherDto dto)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == id && t.Role == UserRole.Teacher);
            if (teacher == null) return ServiceResult<bool>.Failure("Teacher not found.", ServiceError.NotFound);

            if (!string.IsNullOrWhiteSpace(dto.Username)) teacher.Username = dto.Username;
            if (!string.IsNullOrWhiteSpace(dto.Email)) teacher.Email = dto.Email.Trim().ToLower();

            if (dto.SchoolClassId.HasValue)
                teacher.SchoolClassId = dto.SchoolClassId.Value == 0 ? null : dto.SchoolClassId.Value;

            if (dto.Department != null)
                teacher.Department = dto.Department;

            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> DeleteTeacherAsync(long id)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == id && t.Role == UserRole.Teacher);
            if (teacher == null) return ServiceResult<bool>.Failure("Teacher not found.", ServiceError.NotFound);

            teacher.IsArchived = true;
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }
    }
}