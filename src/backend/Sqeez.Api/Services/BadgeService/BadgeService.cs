using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Gamification;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Services
{
    public class BadgeService : BaseService<BadgeService>, IBadgeService
    {
        public BadgeService(SqeezDbContext context, ILogger<BadgeService> logger)
            : base(context, logger) { }

        public async Task<ServiceResult<BadgeDto>> CreateBadgeAsync(CreateBadgeDto dto)
        {
            var badge = new Badge
            {
                Name = dto.Name,
                Description = dto.Description,
                IconUrl = dto.IconUrl,
                XpBonus = dto.XpBonus,
                Condition = dto.Condition
            };

            _context.Badges.Add(badge);
            await _context.SaveChangesAsync();

            return ServiceResult<BadgeDto>.Ok(new BadgeDto(
                badge.Id, badge.Name, badge.Description, badge.IconUrl, badge.XpBonus, badge.Condition));
        }

        public async Task<ServiceResult<BadgeDto>> UpdateBadgeAsync(long id, UpdateBadgeDto dto)
        {
            var badge = await _context.Badges.FindAsync(id);
            if (badge == null) return ServiceResult<BadgeDto>.Failure("Badge not found.", ServiceError.NotFound);

            badge.Name = dto.Name;
            badge.Description = dto.Description;
            badge.IconUrl = dto.IconUrl;
            badge.XpBonus = dto.XpBonus;
            badge.Condition = dto.Condition;

            await _context.SaveChangesAsync();

            return ServiceResult<BadgeDto>.Ok(new BadgeDto(
                badge.Id, badge.Name, badge.Description, badge.IconUrl, badge.XpBonus, badge.Condition));
        }

        public async Task<ServiceResult<bool>> DeleteBadgeAsync(long id)
        {
            var badge = await _context.Badges.FindAsync(id);
            if (badge == null) return ServiceResult<bool>.Failure("Badge not found.", ServiceError.NotFound);

            _context.Badges.Remove(badge);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<IEnumerable<BadgeDto>>> GetAllBadgesAsync()
        {
            var badges = await _context.Badges
                .Select(b => new BadgeDto(b.Id, b.Name, b.Description, b.IconUrl, b.XpBonus, b.Condition))
                .ToListAsync();

            return ServiceResult<IEnumerable<BadgeDto>>.Ok(badges);
        }

        public async Task<ServiceResult<IEnumerable<StudentBadgeDto>>> GetStudentBadgesAsync(long studentId)
        {
            var earnedBadges = await _context.StudentBadges
                .Include(sb => sb.Badge)
                .Where(sb => sb.StudentId == studentId)
                .OrderByDescending(sb => sb.EarnedAt)
                .Select(sb => new StudentBadgeDto(
                    sb.BadgeId, sb.Badge.Name, sb.Badge.Description, sb.Badge.IconUrl, sb.Badge.XpBonus, sb.EarnedAt))
                .ToListAsync();

            return ServiceResult<IEnumerable<StudentBadgeDto>>.Ok(earnedBadges);
        }

        public async Task<ServiceResult<bool>> AwardBadgeToStudentAsync(long studentId, long badgeId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return ServiceResult<bool>.Failure("Student not found.", ServiceError.NotFound);

            var badge = await _context.Badges.FindAsync(badgeId);
            if (badge == null) return ServiceResult<bool>.Failure("Badge not found.", ServiceError.NotFound);

            // Check if they already have it!
            bool alreadyEarned = await _context.StudentBadges
                .AnyAsync(sb => sb.StudentId == studentId && sb.BadgeId == badgeId);

            if (alreadyEarned)
                return ServiceResult<bool>.Failure("Student has already earned this badge.", ServiceError.Conflict);

            // Award the badge
            var studentBadge = new StudentBadge
            {
                StudentId = studentId,
                BadgeId = badgeId,
                EarnedAt = DateTime.UtcNow
            };

            _context.StudentBadges.Add(studentBadge);

            // Grant the XP Bonus to the student profile!
            student.CurrentXP += badge.XpBonus;

            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }
    }
}