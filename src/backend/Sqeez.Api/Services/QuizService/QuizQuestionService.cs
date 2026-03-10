using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.QuizSystem;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Services
{
    public class QuizQuestionService : BaseService<QuizQuestionService>, IQuizQuestionService
    {
        public QuizQuestionService(SqeezDbContext context, ILogger<QuizQuestionService> logger) : base(context, logger) { }

        public async Task<ServiceResult<PagedResponse<QuizQuestionDto>>> GetAllQuizQuestionsAsync(QuizQuestionFilterDto filter)
        {
            var query = _context.QuizQuestions.AsNoTracking();

            if (filter.QuizId.HasValue)
            {
                query = query.Where(q => q.QuizId == filter.QuizId.Value);
            }

            if (filter.Difficulty.HasValue)
            {
                query = query.Where(q => q.Difficulty == filter.Difficulty.Value);
            }

            if (filter.MediaAssetId.HasValue)
            {
                query = query.Where(q => q.MediaAssetId == filter.MediaAssetId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var search = filter.SearchTerm.Trim().ToLower();
                query = query.Where(q => q.Title != null && q.Title.ToLower().Contains(search));
            }

            int totalCount = await query.CountAsync();

            var questions = await query
                .OrderBy(q => q.Id)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(q => new QuizQuestionDto(
                    q.Id,
                    q.Title ?? string.Empty,
                    q.Difficulty,
                    q.TimeLimit,
                    q.QuizId,
                    q.MediaAssetId,
                    q.Options.Count
                ))
                .ToListAsync();

            return ServiceResult<PagedResponse<QuizQuestionDto>>.Ok(new PagedResponse<QuizQuestionDto>
            {
                Data = questions,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            });
        }

        public async Task<ServiceResult<QuizQuestionDto>> GetQuizQuestionByIdAsync(long id)
        {
            var question = await _context.QuizQuestions
                .Where(q => q.Id == id)
                .Select(q => new QuizQuestionDto(
                    q.Id,
                    q.Title ?? string.Empty,
                    q.Difficulty,
                    q.TimeLimit,
                    q.QuizId,
                    q.MediaAssetId,
                    q.Options.Count
                ))
                .FirstOrDefaultAsync();

            if (question == null)
                return ServiceResult<QuizQuestionDto>.Failure("Quiz question not found.", ServiceError.NotFound);

            return ServiceResult<QuizQuestionDto>.Ok(question);
        }

        public async Task<ServiceResult<QuizQuestionDto>> CreateQuizQuestionAsync(CreateQuizQuestionDto dto)
        {
            var quizExists = await _context.Quizzes.AnyAsync(q => q.Id == dto.QuizId);
            if (!quizExists)
                return ServiceResult<QuizQuestionDto>.Failure("The specified Quiz does not exist.", ServiceError.NotFound);

            if (dto.MediaAssetId.HasValue)
            {
                var mediaExists = await _context.MediaAssets.AnyAsync(m => m.Id == dto.MediaAssetId.Value);
                if (!mediaExists)
                    return ServiceResult<QuizQuestionDto>.Failure("The specified Media Asset does not exist.", ServiceError.NotFound);
            }

            var question = new QuizQuestion
            {
                Title = dto.Title,
                Difficulty = dto.Difficulty,
                TimeLimit = dto.TimeLimit,
                QuizId = dto.QuizId,
                MediaAssetId = dto.MediaAssetId
            };

            _context.QuizQuestions.Add(question);
            await _context.SaveChangesAsync();

            return ServiceResult<QuizQuestionDto>.Ok(new QuizQuestionDto(
                question.Id,
                question.Title,
                question.Difficulty,
                question.TimeLimit,
                question.QuizId,
                question.MediaAssetId,
                0)); // 0 Options on initial creation
        }

        public async Task<ServiceResult<QuizQuestionDto>> PatchQuizQuestionAsync(long id, PatchQuizQuestionDto dto)
        {
            var question = await _context.QuizQuestions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return ServiceResult<QuizQuestionDto>.Failure("Quiz question not found.", ServiceError.NotFound);

            if (dto.Title != null) question.Title = dto.Title;
            if (dto.Difficulty.HasValue) question.Difficulty = dto.Difficulty.Value;
            if (dto.TimeLimit.HasValue) question.TimeLimit = dto.TimeLimit.Value;

            if (dto.MediaAssetId.HasValue)
            {
                if (dto.MediaAssetId.Value == 0)
                {
                    question.MediaAssetId = null;
                }
                else if (dto.MediaAssetId.Value != question.MediaAssetId)
                {
                    var mediaExists = await _context.MediaAssets.AnyAsync(m => m.Id == dto.MediaAssetId.Value);
                    if (!mediaExists)
                        return ServiceResult<QuizQuestionDto>.Failure("The specified Media Asset does not exist.", ServiceError.NotFound);

                    question.MediaAssetId = dto.MediaAssetId.Value;
                }
            }

            await _context.SaveChangesAsync();

            return ServiceResult<QuizQuestionDto>.Ok(new QuizQuestionDto(
                question.Id,
                question.Title ?? string.Empty,
                question.Difficulty,
                question.TimeLimit,
                question.QuizId,
                question.MediaAssetId,
                question.Options.Count));
        }

        public async Task<ServiceResult<bool>> DeleteQuizQuestionAsync(long id)
        {
            var question = await _context.QuizQuestions.FindAsync(id);

            if (question == null)
                return ServiceResult<bool>.Failure("Quiz question not found.", ServiceError.NotFound);

            // Hard delete with quiz options cascade delete
            _context.QuizQuestions.Remove(question);

            try
            {
                await _context.SaveChangesAsync();
                return ServiceResult<bool>.Ok(true);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to delete QuizQuestion {Id} due to database constraints.", id);
                return ServiceResult<bool>.Failure("Cannot delete this question because students have already submitted responses to it.", ServiceError.Conflict);
            }
        }
    }
}