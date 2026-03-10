using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqeez.Api.DTOs;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Controllers
{
    [Authorize]
    [Route("api/quizzes")]
    public class QuizzesController : ApiBaseController
    {
        private readonly IQuizService _quizService;
        private readonly IQuizQuestionService _questionService;
        private readonly IQuizOptionService _optionService;

        public QuizzesController(
            IQuizService quizService,
            IQuizQuestionService questionService,
            IQuizOptionService optionService)
        {
            _quizService = quizService;
            _questionService = questionService;
            _optionService = optionService;
        }

        #region --- 1. QUIZZES ---

        [HttpGet("{quizId}")]
        public async Task<ActionResult> GetQuiz(long quizId)
        {
            var result = await _quizService.GetQuizByIdAsync(quizId);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPatch("{quizId}")]
        public async Task<ActionResult> PatchQuiz(long quizId, [FromBody] PatchQuizDto dto)
        {
            var result = await _quizService.PatchQuizAsync(quizId, dto);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpDelete("{quizId}")]
        public async Task<ActionResult> DeleteQuiz(long quizId)
        {
            var result = await _quizService.DeleteQuizAsync(quizId);
            return HandleServiceResult(result);
        }

        #endregion

        #region --- 2. QUIZ QUESTIONS ---

        [HttpGet("{quizId}/questions")]
        public async Task<ActionResult> GetQuestions(long quizId, [FromQuery] QuizQuestionFilterDto filter)
        {
            filter.QuizId = quizId;
            var result = await _questionService.GetAllQuizQuestionsAsync(filter);
            return HandleServiceResult(result);
        }

        [HttpGet("{quizId}/questions/{questionId}")]
        public async Task<ActionResult> GetQuestion(long quizId, long questionId)
        {
            var result = await _questionService.GetQuizQuestionByIdAsync(questionId);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost("{quizId}/questions")]
        public async Task<ActionResult> CreateQuestion(long quizId, [FromBody] CreateQuizQuestionDto dto)
        {
            var safeDto = dto with { QuizId = quizId };
            var result = await _questionService.CreateQuizQuestionAsync(safeDto);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPatch("{quizId}/questions/{questionId}")]
        public async Task<ActionResult> PatchQuestion(long quizId, long questionId, [FromBody] PatchQuizQuestionDto dto)
        {
            var result = await _questionService.PatchQuizQuestionAsync(questionId, dto);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpDelete("{quizId}/questions/{questionId}")]
        public async Task<ActionResult> DeleteQuestion(long quizId, long questionId)
        {
            var result = await _questionService.DeleteQuizQuestionAsync(questionId);
            return HandleServiceResult(result);
        }

        #endregion

        #region --- 3. QUIZ OPTIONS ---

        [HttpGet("{quizId}/questions/{questionId}/options")]
        public async Task<ActionResult> GetOptions(long quizId, long questionId, [FromQuery] QuizOptionFilterDto filter)
        {
            filter.QuizQuestionId = questionId;
            var result = await _optionService.GetAllQuizOptionsAsync(filter);
            return HandleServiceResult(result);
        }

        [HttpGet("{quizId}/questions/{questionId}/options/{optionId}")]
        public async Task<ActionResult> GetOption(long quizId, long questionId, long optionId)
        {
            var result = await _optionService.GetQuizOptionByIdAsync(optionId);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost("{quizId}/questions/{questionId}/options")]
        public async Task<ActionResult> CreateOption(long quizId, long questionId, [FromBody] CreateQuizOptionDto dto)
        {
            var safeDto = dto with { QuizQuestionID = questionId };
            var result = await _optionService.CreateQuizOptionAsync(safeDto);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPatch("{quizId}/questions/{questionId}/options/{optionId}")]
        public async Task<ActionResult> PatchOption(long quizId, long questionId, long optionId, [FromBody] PatchQuizOptionDto dto)
        {
            var result = await _optionService.PatchQuizOptionAsync(optionId, dto);
            return HandleServiceResult(result);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpDelete("{quizId}/questions/{questionId}/options/{optionId}")]
        public async Task<ActionResult> DeleteOption(long quizId, long questionId, long optionId)
        {
            var result = await _optionService.DeleteQuizOptionAsync(optionId);
            return HandleServiceResult(result);
        }

        #endregion
    }
}