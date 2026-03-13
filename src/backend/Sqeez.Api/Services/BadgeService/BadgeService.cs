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
                Rules = dto.Rules.Select(r => new BadgeRule
                {
                    Metric = r.Metric,
                    Operator = r.Operator,
                    TargetValue = r.TargetValue
                }).ToList()
            };

            _context.Badges.Add(badge);
            await _context.SaveChangesAsync();

            var ruleDtos = badge.Rules.Select(r => new BadgeRuleDto(r.Id, r.Metric, r.Operator, r.TargetValue)).ToList();

            return ServiceResult<BadgeDto>.Ok(new BadgeDto(
                badge.Id, badge.Name, badge.Description, badge.IconUrl, badge.XpBonus, ruleDtos));
        }

        public async Task<ServiceResult<BadgeDto>> UpdateBadgeAsync(long id, UpdateBadgeDto dto)
        {
            var badge = await _context.Badges
                .Include(b => b.Rules)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (badge == null) return ServiceResult<BadgeDto>.Failure("Badge not found.", ServiceError.NotFound);

            if (dto.Name != null) badge.Name = dto.Name;
            if (dto.Description != null) badge.Description = dto.Description;
            if (dto.IconUrl != null) badge.IconUrl = dto.IconUrl;
            if (dto.XpBonus.HasValue) badge.XpBonus = dto.XpBonus.Value;

            if (dto.Rules != null)
            {
                var incomingIds = dto.Rules
                    .Where(r => r.Id.HasValue && r.Id > 0)
                    .Select(r => r.Id!.Value)
                    .ToList();

                var rulesToRemove = badge.Rules.Where(r => !incomingIds.Contains(r.Id)).ToList();
                _context.BadgeRules.RemoveRange(rulesToRemove);

                foreach (var ruleDto in dto.Rules)
                {
                    if (!ruleDto.Id.HasValue || ruleDto.Id == 0)
                    {
                        badge.Rules.Add(new BadgeRule
                        {
                            Metric = ruleDto.Metric,
                            Operator = ruleDto.Operator,
                            TargetValue = ruleDto.TargetValue
                        });
                    }
                    else
                    {
                        var existingRule = badge.Rules.FirstOrDefault(r => r.Id == ruleDto.Id);
                        if (existingRule != null)
                        {
                            existingRule.Metric = ruleDto.Metric;
                            existingRule.Operator = ruleDto.Operator;
                            existingRule.TargetValue = ruleDto.TargetValue;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            var ruleDtos = badge.Rules.Select(r => new BadgeRuleDto(r.Id, r.Metric, r.Operator, r.TargetValue)).ToList();

            return ServiceResult<BadgeDto>.Ok(new BadgeDto(
                badge.Id, badge.Name, badge.Description, badge.IconUrl, badge.XpBonus, ruleDtos));
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
                .Include(b => b.Rules)
                .Select(b => new BadgeDto(
                    b.Id,
                    b.Name,
                    b.Description,
                    b.IconUrl,
                    b.XpBonus,
                    b.Rules.Select(r => new BadgeRuleDto(r.Id, r.Metric, r.Operator, r.TargetValue)).ToList()
                ))
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

            bool alreadyEarned = await _context.StudentBadges
                .AnyAsync(sb => sb.StudentId == studentId && sb.BadgeId == badgeId);

            if (alreadyEarned)
                return ServiceResult<bool>.Failure("Student has already earned this badge.", ServiceError.Conflict);

            var studentBadge = new StudentBadge
            {
                StudentId = studentId,
                BadgeId = badgeId,
                EarnedAt = DateTime.UtcNow
            };

            _context.StudentBadges.Add(studentBadge);

            student.CurrentXP += badge.XpBonus;

            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }

        public async Task EvaluateAndAwardBadgesAsync(long studentId, BadgeEvaluationMetrics metrics)
        {
            // Find out which badges the student already owns
            var earnedBadgeIds = await _context.StudentBadges
                .Where(sb => sb.StudentId == studentId)
                .Select(sb => sb.BadgeId)
                .ToListAsync();

            // Only fetch badges that the student does not have yet!
            var availableBadges = await _context.Badges
                .Include(b => b.Rules)
                .Where(b => !earnedBadgeIds.Contains(b.Id))
                .ToListAsync();

            // Evaluate the remaining unearned badges
            foreach (var badge in availableBadges)
            {
                if (!badge.Rules.Any()) continue;

                bool meetsAllRules = true;

                foreach (var rule in badge.Rules)
                {
                    decimal metricValue = rule.Metric switch
                    {
                        BadgeMetric.ScorePercentage => metrics.ScorePercentage,
                        BadgeMetric.TotalScore => metrics.TotalScore,
                        _ => -1
                    };

                    if (metricValue == -1)
                    {
                        meetsAllRules = false;
                        break;
                    }

                    bool ruleMet = rule.Operator switch
                    {
                        BadgeOperator.Equals => metricValue == rule.TargetValue,
                        BadgeOperator.GreaterThan => metricValue > rule.TargetValue,
                        BadgeOperator.GreaterThanOrEqual => metricValue >= rule.TargetValue,
                        BadgeOperator.LessThan => metricValue < rule.TargetValue,
                        BadgeOperator.LessThanOrEqual => metricValue <= rule.TargetValue,
                        _ => false
                    };

                    if (!ruleMet)
                    {
                        meetsAllRules = false;
                        break;
                    }
                }

                if (meetsAllRules)
                {
                    await AwardBadgeToStudentAsync(studentId, badge.Id);
                }
            }
        }
    }
}