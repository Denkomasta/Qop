using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services.UserService;

namespace Sqeez.Api.Services
{
    public class AdminService : BaseService<AdminService>, IAdminService
    {
        public AdminService(SqeezDbContext context, ILogger<AdminService> logger) : base(context, logger) { }

        public async Task<ServiceResult<PagedResponse<AdminDto>>> GetAllAdminsAsync(AdminFilterDto filter)
        {
            var query = _context.Admins.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.Trim().ToLower();
                query = query.Where(a => a.Username.ToLower().Contains(searchTerm) ||
                                         a.Email.ToLower().Contains(searchTerm));
            }

            if (filter.IsOnline is bool isOnline)
            {
                var threshold = DateTime.UtcNow.AddMinutes(-15);

                query = query.Where(a =>
                    isOnline ? a.LastSeen >= threshold : a.LastSeen < threshold);
            }

            if (filter.SchoolClassId.HasValue)
            {
                query = query.Where(a => a.SchoolClassId == filter.SchoolClassId.Value);
            }

            if (filter.IsArchived is true)
            {
                query = query.Where(a => a.ArchivedAt != null);
            }
            else
            {
                query = query.Where(a => a.ArchivedAt == null);
            }

            if (!string.IsNullOrWhiteSpace(filter.Department))
            {
                query = query.Where(a => a.Department == filter.Department);
            }

            if (!string.IsNullOrWhiteSpace(filter.PhoneNumber))
            {
                query = query.Where(a => a.PhoneNumber == filter.PhoneNumber);
            }

            int totalCount = await query.CountAsync();

            var admins = await query
                .OrderBy(a => a.Username)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(a => new AdminDto
                {
                    Id = a.Id,
                    Username = a.Username,
                    Email = a.Email,
                    CurrentXP = a.CurrentXP,
                    Role = a.Role.ToString(),
                    LastSeen = a.LastSeen,
                    SchoolClassId = a.SchoolClassId,
                    Department = a.Department,
                    ManagedClassId = a.ManagedClassId,
                    PhoneNumber = a.PhoneNumber,
                })
                .ToListAsync();

            var response = new PagedResponse<AdminDto>
            {
                Data = admins,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            return ServiceResult<PagedResponse<AdminDto>>.Ok(response);
        }

        public async Task<ServiceResult<AdminDto>> GetAdminByIdAsync(long id)
        {
            var admin = await _context.Admins
                .Where(a => a.Id == id)
                .Select(a => new AdminDto
                {
                    Id = a.Id,
                    Username = a.Username,
                    Email = a.Email,
                    CurrentXP = a.CurrentXP,
                    Role = a.Role.ToString(),
                    LastSeen = a.LastSeen,
                    SchoolClassId = a.SchoolClassId,
                    Department = a.Department,
                    ManagedClassId = a.ManagedClassId,
                    PhoneNumber = a.PhoneNumber
                })
                .FirstOrDefaultAsync();

            if (admin == null) return ServiceResult<AdminDto>.Failure("Admin not found.", ServiceError.NotFound);
            return ServiceResult<AdminDto>.Ok(admin);
        }

        public async Task<ServiceResult<AdminDto>> CreateAdminAsync(CreateAdminDto dto)
        {
            if (await _context.Students.AnyAsync(u => u.Email == dto.Email.Trim().ToLower()))
                return ServiceResult<AdminDto>.Failure("Email already in use.", ServiceError.Conflict);

            if (dto.SchoolClassId.HasValue && dto.SchoolClassId.Value != 0)
            {
                var classExists = await _context.SchoolClasses.AnyAsync(c => c.Id == dto.SchoolClassId.Value);
                if (!classExists) return ServiceResult<AdminDto>.Failure("The specified School Class does not exist.", ServiceError.NotFound);
            }

            if (dto.ManagedClassId.HasValue && dto.ManagedClassId.Value != 0)
            {
                var classExists = await _context.SchoolClasses.AnyAsync(c => c.Id == dto.ManagedClassId.Value);
                if (!classExists) return ServiceResult<AdminDto>.Failure("The specified Managed Class does not exist.", ServiceError.NotFound);
            }

            var admin = new Admin
            {
                Username = dto.Username,
                Email = dto.Email.Trim().ToLower(),
                PasswordHash = dto.Password,    // Password should be already hashed!
                Role = UserRole.Admin,
                LastSeen = DateTime.UtcNow,
                SchoolClassId = dto.SchoolClassId,
                Department = dto.Department,
                ManagedClassId = dto.ManagedClassId,
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? "-" : dto.PhoneNumber
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            var resultDto = new AdminDto
            {
                Id = admin.Id,
                Username = admin.Username,
                Email = admin.Email,
                Role = admin.Role.ToString(),
                SchoolClassId = admin.SchoolClassId,
                Department = admin.Department,
                ManagedClassId= admin.ManagedClassId,
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? "-" : dto.PhoneNumber
            };

            return ServiceResult<AdminDto>.Ok(resultDto);
        }

        public async Task<ServiceResult<AdminDto>> PatchAdminAsync(long id, PatchAdminDto dto)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == id && a.Role == UserRole.Admin);
            if (admin == null) return ServiceResult<AdminDto>.Failure("Admin not found.", ServiceError.NotFound);

            if (dto.SchoolClassId.HasValue && dto.SchoolClassId.Value != 0)
            {
                var classExists = await _context.SchoolClasses.AnyAsync(c => c.Id == dto.SchoolClassId.Value);
                if (!classExists) return ServiceResult<AdminDto>.Failure("The specified School Class does not exist.", ServiceError.NotFound);

                admin.SchoolClassId = dto.SchoolClassId.Value;
            }
            else if (dto.SchoolClassId == 0)
            {
                admin.SchoolClassId = null;
            }

            if (dto.ManagedClassId.HasValue && dto.ManagedClassId.Value != 0)
            {
                var classExists = await _context.SchoolClasses.AnyAsync(c => c.Id == dto.ManagedClassId.Value);
                if (!classExists) return ServiceResult<AdminDto>.Failure("The specified Managed Class does not exist.", ServiceError.NotFound);

                admin.ManagedClassId = dto.ManagedClassId.Value;
            }
            else if (dto.ManagedClassId == 0)
            {
                admin.ManagedClassId = null;
            }

            if (!string.IsNullOrWhiteSpace(dto.Username)) admin.Username = dto.Username;
            if (!string.IsNullOrWhiteSpace(dto.Email)) admin.Email = dto.Email.Trim().ToLower();

            if (dto.SchoolClassId.HasValue)
                admin.SchoolClassId = dto.SchoolClassId.Value == 0 ? null : dto.SchoolClassId.Value;

            if (dto.Department != null) admin.Department = dto.Department;

            if (dto.PhoneNumber != null)
                admin.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? "-" : dto.PhoneNumber;

            await _context.SaveChangesAsync();

            var resultDto = new AdminDto
            {
                Id = admin.Id,
                Username = admin.Username,
                Email = admin.Email,
                Role = admin.Role.ToString(),
                SchoolClassId = admin.SchoolClassId,
                Department = admin.Department,
                ManagedClassId = admin.ManagedClassId,
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? "-" : dto.PhoneNumber
            };

            return ServiceResult<AdminDto>.Ok(resultDto);
        }

        public async Task<ServiceResult<bool>> DeleteAdminAsync(long id)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == id && a.Role == UserRole.Admin);
            if (admin == null) return ServiceResult<bool>.Failure("Admin not found.", ServiceError.NotFound);

            admin.ArchivedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }
    }
}