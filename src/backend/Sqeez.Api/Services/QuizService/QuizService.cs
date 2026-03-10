using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.QuizSystem;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Services
{
    public class QuizService : BaseService<QuizService>, IQuizService
    {
        public QuizService(SqeezDbContext context, ILogger<QuizService> logger) : base(context, logger) { }

        public async Task<ServiceResult<PagedResponse<QuizDto>>> GetAllQuizzesAsync(QuizFilterDto filter)
        {
            var query = _context.Quizzes.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var search = filter.SearchTerm.Trim().ToLower();
                query = query.Where(q => q.Title.ToLower().Contains(search) ||
                                         q.Description.ToLower().Contains(search));
            }

            if (filter.SubjectId.HasValue)
            {
                query = query.Where(q => q.SubjectId == filter.SubjectId.Value);
            }

            if (filter.IsActive.HasValue)
            {
                var now = DateTime.UtcNow;
                if (filter.IsActive.Value)
                {
                    // Active: Published in the past, and (No closing date OR closing date is in the future)
                    query = query.Where(q => q.PublishDate != null &&
                                             q.PublishDate <= now &&
                                             (q.ClosingDate == null || q.ClosingDate > now));
                }
                else
                {
                    // Inactive: Not published, published in future, or closing date has passed
                    query = query.Where(q => q.PublishDate == null ||
                                             q.PublishDate > now ||
                                             (q.ClosingDate != null && q.ClosingDate <= now));
                }
            }

            if (filter.PublishDate.HasValue) query = query.Where(q => q.PublishDate >= filter.PublishDate.Value);
            if (filter.ClosingDate.HasValue) query = query.Where(q => q.ClosingDate <= filter.ClosingDate.Value);

            int totalCount = await query.CountAsync();

            var quizzes = await query
                .OrderByDescending(q => q.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(q => new QuizDto(
                    q.Id,
                    q.Title,
                    q.Description,
                    q.MaxRetries,
                    q.CreatedAt,
                    q.PublishDate,
                    q.ClosingDate,
                    q.SubjectId,
                    q.QuizQuestions.Count,
                    q.QuizAttempts.Count
                ))
                .ToListAsync();

            return ServiceResult<PagedResponse<QuizDto>>.Ok(new PagedResponse<QuizDto>
            {
                Data = quizzes,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            });
        }

        public async Task<ServiceResult<QuizDto>> GetQuizByIdAsync(long id)
        {
            var quiz = await _context.Quizzes
                .Where(q => q.Id == id)
                .Select(q => new QuizDto(
                    q.Id,
                    q.Title,
                    q.Description,
                    q.MaxRetries,
                    q.CreatedAt,
                    q.PublishDate,
                    q.ClosingDate,
                    q.SubjectId,
                    q.QuizQuestions.Count,
                    q.QuizAttempts.Count
                ))
                .FirstOrDefaultAsync();

            if (quiz == null) return ServiceResult<QuizDto>.Failure("Quiz not found.", ServiceError.NotFound);

            return ServiceResult<QuizDto>.Ok(quiz);
        }

        public async Task<ServiceResult<QuizDto>> CreateQuizAsync(CreateQuizDto dto)
        {
            var subjectExists = await _context.Subjects.AnyAsync(s => s.Id == dto.SubjectId);
            if (!subjectExists) return ServiceResult<QuizDto>.Failure("Subject not found.", ServiceError.NotFound);

            var quiz = new Quiz
            {
                Title = dto.Title,
                Description = dto.Description,
                MaxRetries = dto.MaxRetries,
                SubjectId = dto.SubjectId,
                PublishDate = dto.PublishDate,
                ClosingDate = dto.ClosingDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return ServiceResult<QuizDto>.Ok(new QuizDto(
                quiz.Id, quiz.Title, quiz.Description, quiz.MaxRetries, quiz.CreatedAt,
                quiz.PublishDate, quiz.ClosingDate, quiz.SubjectId, 0, 0));
        }

        public async Task<ServiceResult<QuizDto>> PatchQuizAsync(long id, PatchQuizDto dto)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.QuizQuestions)
                .Include(q => q.QuizAttempts)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return ServiceResult<QuizDto>.Failure("Quiz not found.", ServiceError.NotFound);

            if (!string.IsNullOrWhiteSpace(dto.Title)) quiz.Title = dto.Title;
            if (dto.Description != null) quiz.Description = dto.Description;
            if (dto.MaxRetries.HasValue) quiz.MaxRetries = dto.MaxRetries.Value;
            if (dto.PublishDate.HasValue) quiz.PublishDate = dto.PublishDate.Value;
            if (dto.ClosingDate.HasValue) quiz.ClosingDate = dto.ClosingDate.Value;

            if (dto.SubjectId.HasValue && dto.SubjectId.Value != quiz.SubjectId)
            {
                var subjectExists = await _context.Subjects.AnyAsync(s => s.Id == dto.SubjectId.Value);
                if (!subjectExists) return ServiceResult<QuizDto>.Failure("Subject not found.", ServiceError.NotFound);
                quiz.SubjectId = dto.SubjectId.Value;
            }

            await _context.SaveChangesAsync();

            return ServiceResult<QuizDto>.Ok(new QuizDto(
                quiz.Id, quiz.Title, quiz.Description, quiz.MaxRetries, quiz.CreatedAt,
                quiz.PublishDate, quiz.ClosingDate, quiz.SubjectId, quiz.QuizQuestions.Count, quiz.QuizAttempts.Count));
        }

        public async Task<ServiceResult<bool>> DeleteQuizAsync(long id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.QuizAttempts)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return ServiceResult<bool>.Failure("Quiz not found.", ServiceError.NotFound);

            if (quiz.QuizAttempts.Any())
            {
                // Soft Delete: If students have already taken it, we just close it so history is preserved.
                quiz.ClosingDate = DateTime.UtcNow;
                _logger.LogInformation("Soft deleted Quiz {Id} by setting ClosingDate to now.", id);
            }
            else
            {
                // Hard Delete: Safe to wipe completely (cascade rules will delete questions and options)
                _context.Quizzes.Remove(quiz);
                _logger.LogInformation("Hard deleted Quiz {Id}.", id);
            }

            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }
    }
}