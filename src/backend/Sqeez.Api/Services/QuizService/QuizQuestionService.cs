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
        private readonly IMediaAssetService _mediaAssetService;

        public QuizQuestionService(SqeezDbContext context, ILogger<QuizQuestionService> logger, IMediaAssetService mas) : base(context, logger)
        {
            _mediaAssetService = mas;
        }

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
                    q.Penalty,
                    q.TimeLimit,
                    q.IsStrictMultipleChoice,
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
                    q.Penalty,
                    q.TimeLimit,
                    q.IsStrictMultipleChoice,
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
            var quizData = await _context.Quizzes
                .Where(q => q.Id == dto.QuizId)
                .Select(q => new
                {
                    HasAttempts = _context.QuizAttempts.Any(a => a.QuizId == q.Id)
                })
                .FirstOrDefaultAsync();

            if (quizData == null)
                return ServiceResult<QuizQuestionDto>.Failure("The specified Quiz does not exist.", ServiceError.NotFound);

            if (quizData.HasAttempts)
            {
                return ServiceResult<QuizQuestionDto>.Failure(
                    "Cannot add new questions to this quiz because students have already started or completed it.",
                    ServiceError.Conflict);
            }

            var question = new QuizQuestion
            {
                Title = dto.Title,
                Difficulty = dto.Difficulty,
                Penalty = dto.Penalty,
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
                question.Penalty,
                question.TimeLimit,
                question.IsStrictMultipleChoice,
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

            bool hasAttempts = await _context.QuizAttempts.AnyAsync(a => a.QuizId == question.QuizId);
            if (hasAttempts)
            {
                return ServiceResult<QuizQuestionDto>.Failure(
                    "Cannot modify this question because students have already started or completed this quiz.",
                    ServiceError.Conflict);
            }

            if (dto.Title != null) question.Title = dto.Title;
            if (dto.Difficulty.HasValue) question.Difficulty = dto.Difficulty.Value;
            if (dto.Penalty.HasValue) question.Penalty = dto.Penalty.Value;
            if (dto.TimeLimit.HasValue) question.TimeLimit = dto.TimeLimit.Value;
            if (dto.IsStrictMultipleChoice.HasValue) question.IsStrictMultipleChoice = dto.IsStrictMultipleChoice.Value;

            long? oldMediaAssetId = null;

            if (dto.MediaAssetId.HasValue)
            {
                if (dto.MediaAssetId.Value == 0)
                {
                    if (question.MediaAssetId != null)
                    {
                        oldMediaAssetId = question.MediaAssetId;
                        question.MediaAssetId = null;
                    }
                }
                else if (dto.MediaAssetId.Value != question.MediaAssetId)
                {
                    var mediaExists = await _context.MediaAssets.AnyAsync(m => m.Id == dto.MediaAssetId.Value);
                    if (!mediaExists)
                        return ServiceResult<QuizQuestionDto>.Failure("The specified Media Asset does not exist.", ServiceError.NotFound);

                    oldMediaAssetId = question.MediaAssetId;
                    question.MediaAssetId = dto.MediaAssetId.Value;
                }
            }

            await _context.SaveChangesAsync();

            if (oldMediaAssetId.HasValue)
            {
                await _mediaAssetService.DeleteMediaAssetAndFileAsync(oldMediaAssetId.Value);
            }

            return ServiceResult<QuizQuestionDto>.Ok(new QuizQuestionDto(
                question.Id,
                question.Title ?? string.Empty,
                question.Difficulty,
                question.Penalty,
                question.TimeLimit,
                question.IsStrictMultipleChoice,
                question.QuizId,
                question.MediaAssetId,
                question.Options.Count));
        }

        public async Task<ServiceResult<bool>> DeleteQuizQuestionAsync(long id)
        {
            var question = await _context.QuizQuestions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return ServiceResult<bool>.Failure("Quiz question not found.", ServiceError.NotFound);

            bool hasAttempts = await _context.QuizAttempts.AnyAsync(a => a.QuizId == question.QuizId);
            if (hasAttempts)
            {
                return ServiceResult<bool>.Failure(
                    "Cannot delete this question because students have already started or completed this quiz.",
                    ServiceError.Conflict);
            }

            var mediaAssetIdsToDelete = new List<long>();

            if (question.MediaAssetId.HasValue)
                mediaAssetIdsToDelete.Add(question.MediaAssetId.Value);

            foreach (var option in question.Options)
            {
                if (option.MediaAssetId.HasValue)
                    mediaAssetIdsToDelete.Add(option.MediaAssetId.Value);
            }

            _context.QuizQuestions.Remove(question);

            try
            {
                await _context.SaveChangesAsync();

                foreach (var assetId in mediaAssetIdsToDelete)
                {
                    await _mediaAssetService.DeleteMediaAssetAndFileAsync(assetId);
                }

                return ServiceResult<bool>.Ok(true);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to delete QuizQuestion {Id} due to database constraints.", id);
                return ServiceResult<bool>.Failure("Cannot delete this question because students have already submitted responses to it.", ServiceError.Conflict);
            }
        }

        public async Task<ServiceResult<DetailedQuizQuestionDto>> GetDetailedQuizQuestionByIdAsync(long id, long quizId)
        {
            var question = await _context.QuizQuestions
                .Where(q => q.Id == id)
                .Select(q => new DetailedQuizQuestionDto(
                    q.Id,
                    q.Title ?? string.Empty,
                    q.Difficulty,
                    q.Penalty,
                    q.TimeLimit,
                    q.IsStrictMultipleChoice,
                    q.QuizId,
                    q.MediaAssetId,
                    q.Options.Select(o => new StudentQuizOptionDto(
                        o.Id,
                        o.IsFreeText ? null : o.Text,
                        o.IsFreeText,
                        o.QuizQuestionId,
                        o.MediaAssetId
                    )).ToList()
                ))
                .FirstOrDefaultAsync();

            if (question == null || question.QuizId != quizId)
                return ServiceResult<DetailedQuizQuestionDto>.Failure("Quiz question not found.", ServiceError.NotFound);

            return ServiceResult<DetailedQuizQuestionDto>.Ok(question);
        }
    }
}