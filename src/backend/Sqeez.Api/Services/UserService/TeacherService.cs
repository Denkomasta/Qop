using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services.UserService;

namespace Sqeez.Api.Services
{
    public class TeacherService : BaseService<TeacherService>, ITeacherService
    {
        public TeacherService(SqeezDbContext context, ILogger<TeacherService> logger) : base(context, logger) { }

        public async Task<ServiceResult<PagedResponse<TeacherDto>>> GetAllTeachersAsync(TeacherFilterDto filter)
        {
            var query = _context.Teachers.AsNoTracking();

            if (filter.StrictRoleOnly)
            {
                query = query.Where(t => t.Role == UserRole.Teacher);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.Trim().ToLower();
                query = query.Where(t => t.Username.ToLower().Contains(searchTerm) ||
                                         t.Email.ToLower().Contains(searchTerm));
            }

            if (filter.IsOnline.HasValue)
            {
                query = query.Where(t => t.IsOnline == filter.IsOnline.Value);
            }

            if (filter.SchoolClassId.HasValue)
            {
                query = query.Where(t => t.SchoolClassId == filter.SchoolClassId.Value);
            }

            if (filter.IsArchived == true)
            {
                query = query.Where(t => t.ArchivedAt != null);
            }
            else
            {
                query = query.Where(t => t.ArchivedAt == null);
            }

            if (!string.IsNullOrWhiteSpace(filter.Department))
            {
                query = query.Where(t => t.Department == filter.Department);
            }

            int totalCount = await query.CountAsync();

            var teachers = await query
                .OrderBy(t => t.Username)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(t => new TeacherDto
                {
                    Id = t.Id,
                    Username = t.Username,
                    Email = t.Email,
                    CurrentXP = t.CurrentXP,
                    Role = t.Role.ToString(),
                    IsOnline = t.IsOnline,
                    SchoolClassId = t.SchoolClassId,
                    Department = t.Department,
                    ManagedClassId = t.ManagedClassId,
                })
                .ToListAsync();

            var response = new PagedResponse<TeacherDto>
            {
                Data = teachers,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            return ServiceResult<PagedResponse<TeacherDto>>.Ok(response);
        }

        public async Task<ServiceResult<TeacherDto>> GetTeacherByIdAsync(long id)
        {
            var teacher = await _context.Teachers
                .Where(t => t.Id == id)
                .Select(t => new TeacherDto
                {
                    Id = t.Id,
                    Username = t.Username,
                    Email = t.Email,
                    CurrentXP = t.CurrentXP,
                    Role = t.Role.ToString(),
                    IsOnline = t.IsOnline,
                    SchoolClassId = t.SchoolClassId,
                    Department = t.Department,
                    ManagedClassId= t.ManagedClassId,
                })
                .FirstOrDefaultAsync();

            if (teacher == null) return ServiceResult<TeacherDto>.Failure("Teacher not found.", ServiceError.NotFound);
            return ServiceResult<TeacherDto>.Ok(teacher);
        }

        public async Task<ServiceResult<TeacherDto>> CreateTeacherAsync(CreateTeacherDto dto)
        {
            if (await _context.Students.AnyAsync(u => u.Email == dto.Email.Trim().ToLower()))
                return ServiceResult<TeacherDto>.Failure("Email already in use.", ServiceError.Conflict);

            if (dto.SchoolClassId.HasValue && dto.SchoolClassId.Value != 0)
            {
                var classExists = await _context.SchoolClasses.AnyAsync(c => c.Id == dto.SchoolClassId.Value);
                if (!classExists) return ServiceResult<TeacherDto>.Failure("The specified School Class does not exist.", ServiceError.NotFound);
            }

            if (dto.ManagedClassId.HasValue && dto.ManagedClassId.Value != 0)
            {
                var classExists = await _context.SchoolClasses.AnyAsync(c => c.Id == dto.ManagedClassId.Value);
                if (!classExists) return ServiceResult<TeacherDto>.Failure("The specified Managed Class does not exist.", ServiceError.NotFound);
            }

            var teacher = new Teacher
            {
                Username = dto.Username,
                Email = dto.Email.Trim().ToLower(),
                PasswordHash = dto.Password,    // Password should be already hashed!
                Role = UserRole.Teacher,
                LastSeen = DateTime.UtcNow,
                SchoolClassId = dto.SchoolClassId,
                Department = dto.Department,
                ManagedClassId = dto.ManagedClassId,
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
                Department = teacher.Department,
                ManagedClassId= teacher.ManagedClassId,
            };

            return ServiceResult<TeacherDto>.Ok(resultDto);
        }

        public async Task<ServiceResult<TeacherDto>> PatchTeacherAsync(long id, PatchTeacherDto dto)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == id && t.Role == UserRole.Teacher);
            if (teacher == null) return ServiceResult<TeacherDto>.Failure("Teacher not found.", ServiceError.NotFound);

            if (dto.SchoolClassId.HasValue && dto.SchoolClassId.Value != 0)
            {
                var classExists = await _context.SchoolClasses.AnyAsync(c => c.Id == dto.SchoolClassId.Value);
                if (!classExists) return ServiceResult<TeacherDto>.Failure("The specified School Class does not exist.", ServiceError.NotFound);

                teacher.SchoolClassId = dto.SchoolClassId.Value;
            }
            else if (dto.SchoolClassId == 0)
            {
                teacher.SchoolClassId = null;
            }

            if (dto.ManagedClassId.HasValue && dto.ManagedClassId.Value != 0)
            {
                var classExists = await _context.SchoolClasses.AnyAsync(c => c.Id == dto.ManagedClassId.Value);
                if (!classExists) return ServiceResult<TeacherDto>.Failure("The specified Managed Class does not exist.", ServiceError.NotFound);

                teacher.ManagedClassId = dto.ManagedClassId.Value;
            }
            else if (dto.ManagedClassId == 0)
            {
                teacher.ManagedClassId = null;
            }

            if (!string.IsNullOrWhiteSpace(dto.Username)) teacher.Username = dto.Username;
            if (!string.IsNullOrWhiteSpace(dto.Email)) teacher.Email = dto.Email.Trim().ToLower();

            if (dto.SchoolClassId.HasValue)
                teacher.SchoolClassId = dto.SchoolClassId.Value == 0 ? null : dto.SchoolClassId.Value;

            if (dto.Department != null)
                teacher.Department = dto.Department;

            await _context.SaveChangesAsync();

            var resultDto = new TeacherDto
            {
                Id = teacher.Id,
                Username = teacher.Username,
                Email = teacher.Email,
                Role = teacher.Role.ToString(),
                SchoolClassId = teacher.SchoolClassId,
                Department = teacher.Department,
                ManagedClassId = teacher.ManagedClassId,
            };

            return ServiceResult<TeacherDto>.Ok(resultDto);
        }

        public async Task<ServiceResult<bool>> DeleteTeacherAsync(long id)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == id && t.Role == UserRole.Teacher);
            if (teacher == null) return ServiceResult<bool>.Failure("Teacher not found.", ServiceError.NotFound);

            teacher.ArchivedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }
    }
}