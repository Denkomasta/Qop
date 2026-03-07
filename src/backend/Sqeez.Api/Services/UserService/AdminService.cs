using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services.UserService;
using BC = BCrypt.Net.BCrypt;

namespace Sqeez.Api.Services
{
    public class AdminService : BaseService<AdminService>, IAdminService
    {
        public AdminService(SqeezDbContext context, ILogger<AdminService> logger) : base(context, logger) { }

        public async Task<ServiceResult<PagedResponse<AdminDto>>> GetAllAdminsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Admins.AsNoTracking();

            int totalCount = await query.CountAsync();

            var admins = await query
                .OrderBy(a => a.Username)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AdminDto
                {
                    Id = a.Id,
                    Username = a.Username,
                    Email = a.Email,
                    CurrentXP = a.CurrentXP,
                    Role = a.Role.ToString(),
                    IsOnline = a.IsOnline,
                    SchoolClassId = a.SchoolClassId,
                    Department = a.Department,
                    PhoneNumber = a.PhoneNumber
                })
                .ToListAsync();

            var response = new PagedResponse<AdminDto>
            {
                Data = admins,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<PagedResponse<AdminDto>>.Ok(response);
        }

        public async Task<ServiceResult<AdminDto>> GetAdminByIdAsync(long id)
        {
            var admin = await _context.Admins
                .Where(a => a.Id == id && a.Role == UserRole.Admin)
                .Select(a => new AdminDto
                {
                    Id = a.Id,
                    Username = a.Username,
                    Email = a.Email,
                    CurrentXP = a.CurrentXP,
                    Role = a.Role.ToString(),
                    IsOnline = a.IsOnline,
                    SchoolClassId = a.SchoolClassId,
                    Department = a.Department,
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

            var admin = new Admin
            {
                Username = dto.Username,
                Email = dto.Email.Trim().ToLower(),
                PasswordHash = BC.HashPassword(dto.Password),
                Role = UserRole.Admin,
                LastSeen = DateTime.UtcNow,
                SchoolClassId = dto.SchoolClassId,
                Department = dto.Department,
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
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? "-" : dto.PhoneNumber
            };

            return ServiceResult<AdminDto>.Ok(resultDto);
        }

        public async Task<ServiceResult<bool>> UpdateAdminAsync(long id, UpdateAdminDto dto)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == id && a.Role == UserRole.Admin);
            if (admin == null) return ServiceResult<bool>.Failure("Admin not found.", ServiceError.NotFound);

            admin.Username = dto.Username;
            admin.Email = dto.Email.Trim().ToLower();
            admin.SchoolClassId = dto.SchoolClassId;
            admin.Department = dto.Department;
            admin.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? "-" : dto.PhoneNumber;

            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<bool>> DeleteAdminAsync(long id)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == id && a.Role == UserRole.Admin);
            if (admin == null) return ServiceResult<bool>.Failure("Admin not found.", ServiceError.NotFound);

            admin.IsArchived = true;
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }
    }
}