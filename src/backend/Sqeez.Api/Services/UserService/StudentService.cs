using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;

namespace Sqeez.Api.Services.UserService
{
    public class StudentService : BaseService<StudentService>, IStudentService
    {
        public StudentService(SqeezDbContext context, ILogger<StudentService> logger) : base(context, logger) { }

        public async Task<ServiceResult<PagedResponse<StudentDto>>> GetAllStudentsAsync(StudentFilterDto filter)
        {
            var query = _context.Students.AsNoTracking();

            if (filter.StrictRoleOnly)
            {
                query = query.Where(t => t.Role == UserRole.Student);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.Trim().ToLower();
                query = query.Where(s => s.Username.ToLower().Contains(searchTerm) ||
                                         s.Email.ToLower().Contains(searchTerm));
            }

            if (filter.IsOnline.HasValue)
            {
                query = query.Where(s => s.IsOnline == filter.IsOnline.Value);
            }

            if (filter.SchoolClassId.HasValue)
            {
                query = query.Where(s => s.SchoolClassId == filter.SchoolClassId.Value);
            }

            if (filter.IsArchived == true)
            {
                query = query.Where(s => s.ArchivedAt != null);
            }
            else
            {
                query = query.Where(s => s.ArchivedAt == null);
            }

            int totalCount = await query.CountAsync();

            var students = await query
                .OrderBy(s => s.Username)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
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
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
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
                Username = dto.Username.Trim(),
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

        public async Task<ServiceResult<bool>> PatchStudentAsync(long id, PatchStudentDto dto)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id && s.Role == UserRole.Student);
            if (student == null) return ServiceResult<bool>.Failure("Student not found.", ServiceError.NotFound);

            if (!string.IsNullOrWhiteSpace(dto.Username))
                student.Username = dto.Username;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                student.Email = dto.Email.Trim().ToLower();

            if (dto.SchoolClassId.HasValue)
                student.SchoolClassId = dto.SchoolClassId.Value == 0 ? null : dto.SchoolClassId.Value;

            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> DeleteStudentAsync(long id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id && s.Role == UserRole.Student);
            if (student == null) return ServiceResult<bool>.Failure("Student not found.", ServiceError.NotFound);

            // TODO for now only archive
            student.ArchivedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }
    }
}