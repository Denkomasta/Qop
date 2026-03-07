using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Academics;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Services
{
    public class SchoolClassService : BaseService<SchoolClassService>, ISchoolClassService
    {
        public SchoolClassService(SqeezDbContext context, ILogger<SchoolClassService> logger) : base(context, logger)
        {
        }

        public async Task<ServiceResult<PagedResponse<SchoolClassDto>>> GetAllClassesAsync(SchoolClassFilterDto filter)
        {
            _logger.LogInformation("Fetching school classes with filters.");

            try
            {
                var query = _context.SchoolClasses.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.Trim().ToLower();
                    query = query.Where(c =>
                        c.Name.ToLower().Contains(searchTerm) ||
                        c.Section.ToLower().Contains(searchTerm));
                }

                if (!string.IsNullOrWhiteSpace(filter.AcademicYear))
                {
                    query = query.Where(c => c.AcademicYear == filter.AcademicYear.Trim());
                }

                if (filter.TeacherId.HasValue)
                {
                    query = query.Where(c => c.TeacherId == filter.TeacherId.Value);
                }

                int totalCount = await query.CountAsync();

                var classes = await query
                    .Include(c => c.Teacher)
                    .Include(c => c.Students)
                    .OrderBy(c => c.Name)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(c => new SchoolClassDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        AcademicYear = c.AcademicYear,
                        Section = c.Section,
                        TeacherId = c.TeacherId,
                        TeacherName = c.Teacher != null ? c.Teacher.Username : null,
                        StudentCount = c.Students.Count
                    })
                    .ToListAsync();

                var pagedResponse = new PagedResponse<SchoolClassDto>
                {
                    Data = classes,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };

                return ServiceResult<PagedResponse<SchoolClassDto>>.Ok(pagedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filtered school classes");
                return ServiceResult<PagedResponse<SchoolClassDto>>.Failure("Internal error occurred.", ServiceError.InternalError);
            }
        }

        public async Task<ServiceResult<SchoolClassDto>> GetClassByIdAsync(long id)
        {
            _logger.LogInformation("Fetching school class with ID: {Id}", id);

            var schoolClass = await _context.SchoolClasses
                .Include(c => c.Teacher)
                .Include(c => c.Students)
                .Where(c => c.Id == id)
                .Select(c => new SchoolClassDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    AcademicYear = c.AcademicYear,
                    Section = c.Section,
                    TeacherId = c.TeacherId,
                    TeacherName = c.Teacher != null ? c.Teacher.Username : null,
                    StudentCount = c.Students.Count
                })
                .FirstOrDefaultAsync();

            if (schoolClass == null)
            {
                _logger.LogWarning("School class with ID {Id} not found.", id);
                return ServiceResult<SchoolClassDto>.Failure("Class not found.", ServiceError.NotFound);
            }

            return ServiceResult<SchoolClassDto>.Ok(schoolClass);
        }

        public async Task<ServiceResult<SchoolClassDto>> CreateClassAsync(CreateSchoolClassDto dto)
        {
            _logger.LogInformation("Attempting to create a new school class: {Name} - {Section}", dto.Name, dto.Section);

            var schoolClass = new SchoolClass
            {
                Name = dto.Name,
                AcademicYear = dto.AcademicYear,
                Section = dto.Section,
                TeacherId = dto.TeacherId
            };

            try
            {
                _context.SchoolClasses.Add(schoolClass);
                await _context.SaveChangesAsync();

                var createdDto = new SchoolClassDto
                {
                    Id = schoolClass.Id,
                    Name = schoolClass.Name,
                    AcademicYear = schoolClass.AcademicYear,
                    Section = schoolClass.Section,
                    TeacherId = schoolClass.TeacherId,
                    StudentCount = 0 // New class has no students yet
                };

                _logger.LogInformation("Successfully created school class {Id}", schoolClass.Id);
                return ServiceResult<SchoolClassDto>.Ok(createdDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating school class {Name}", dto.Name);
                return ServiceResult<SchoolClassDto>.Failure("Internal error occurred while creating class.", ServiceError.InternalError);
            }
        }

        public async Task<ServiceResult<SchoolClassDto>> PatchClassAsync(long id, PatchSchoolClassDto dto)
        {
            _logger.LogInformation("Attempting to patch school class with ID: {Id}", id);

            var schoolClass = await _context.SchoolClasses
                .Include(c => c.Students)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (schoolClass == null)
            {
                return ServiceResult<SchoolClassDto>.Failure("Class not found.", ServiceError.NotFound);
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                schoolClass.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.AcademicYear))
                schoolClass.AcademicYear = dto.AcademicYear;

            if (!string.IsNullOrWhiteSpace(dto.Section))
                schoolClass.Section = dto.Section;

            if (dto.TeacherId.HasValue)
            {
                // TeacherId = 0 means "Remove the teacher"
                if (dto.TeacherId.Value == 0)
                {
                    schoolClass.TeacherId = null;
                    schoolClass.Teacher = null;
                }
                else if (dto.TeacherId.Value != schoolClass.TeacherId)
                {
                    var newTeacher = await _context.Teachers.FindAsync(dto.TeacherId.Value);
                    if (newTeacher == null)
                    {
                        return ServiceResult<SchoolClassDto>.Failure("Provided Teacher ID is invalid.", ServiceError.ValidationFailed);
                    }
                    schoolClass.Teacher = newTeacher;
                    schoolClass.TeacherId = dto.TeacherId.Value;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully patched school class {Id}", id);

                var updatedDto = new SchoolClassDto
                {
                    Id = schoolClass.Id,
                    Name = schoolClass.Name,
                    AcademicYear = schoolClass.AcademicYear,
                    Section = schoolClass.Section,
                    TeacherId = schoolClass.TeacherId,
                    TeacherName = schoolClass.Teacher?.Username,
                    StudentCount = schoolClass.Students.Count
                };

                return ServiceResult<SchoolClassDto>.Ok(updatedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error patching school class {Id}", id);
                return ServiceResult<SchoolClassDto>.Failure("Internal error occurred while patching class.", ServiceError.InternalError);
            }
        }

        public async Task<ServiceResult<bool>> DeleteClassAsync(long id)
        {
            _logger.LogInformation("Attempting to delete school class with ID: {Id}", id);

            var schoolClass = await _context.SchoolClasses.FindAsync(id);
            if (schoolClass == null)
            {
                _logger.LogWarning("Deletion failed: School class with ID {Id} not found.", id);
                return ServiceResult<bool>.Failure("Class not found.", ServiceError.NotFound);
            }

            try
            {
                _context.SchoolClasses.Remove(schoolClass);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted school class {Id}", id);
                return ServiceResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting school class {Id}", id);
                return ServiceResult<bool>.Failure("Internal error occurred while deleting class.", ServiceError.InternalError);
            }
        }
    }
}