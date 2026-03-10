using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Models.QuizSystem;
using Sqeez.Api.Services;

namespace Sqeez.Api.Tests.Services
{
    public class QuizQuestionServiceTests
    {
        private async Task<SqeezDbContext> GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SqeezDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new SqeezDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return context;
        }

        private QuizQuestionService CreateService(SqeezDbContext context)
        {
            var mockLogger = new Mock<ILogger<QuizQuestionService>>();
            return new QuizQuestionService(context, mockLogger.Object);
        }

        [Fact]
        public async Task CreateQuizQuestionAsync_WhenValidQuiz_CreatesQuestion()
        {
            var context = await GetInMemoryDbContext();
            var quiz = new Quiz { Title = "Parent Quiz" };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new CreateQuizQuestionDto("What is 2+2?", 1, 30, quiz.Id);

            var result = await service.CreateQuizQuestionAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("What is 2+2?", result.Data!.Title);
            Assert.Equal(quiz.Id, result.Data.QuizId);
        }

        [Fact]
        public async Task PatchQuizQuestionAsync_WhenValid_UpdatesFields()
        {
            var context = await GetInMemoryDbContext();
            var question = new QuizQuestion { Title = "Old Title", Difficulty = 1, TimeLimit = 10, QuizId = 1 };
            context.QuizQuestions.Add(question);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var patchDto = new PatchQuizQuestionDto(Title: "New Title", Difficulty: 5);

            var result = await service.PatchQuizQuestionAsync(question.Id, patchDto);

            Assert.True(result.Success);
            Assert.Equal("New Title", result.Data!.Title);
            Assert.Equal(5, result.Data.Difficulty);
            Assert.Equal(10, result.Data.TimeLimit); // Remains unchanged
        }
    }
}