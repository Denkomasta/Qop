using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Models.QuizSystem;
using Sqeez.Api.Services;
using Sqeez.Api.Services.Interfaces;

namespace Sqeez.Api.Tests.Services
{
    public class QuizOptionServiceTests
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

        private QuizOptionService CreateService(SqeezDbContext context)
        {
            var mockLogger = new Mock<ILogger<QuizOptionService>>();
            var mockMediaAssetService = new Mock<IMediaAssetService>();

            return new QuizOptionService(context, mockLogger.Object, mockMediaAssetService.Object);
        }

        [Fact]
        public async Task CreateQuizOptionAsync_WhenValidQuestion_CreatesOption()
        {
            var context = await GetInMemoryDbContext();
            var question = new QuizQuestion { Title = "Parent Question", QuizId = 1 };
            context.QuizQuestions.Add(question);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new CreateQuizOptionDto(IsCorrect: true, QuizQuestionID: question.Id, Text: "Correct Answer");

            var result = await service.CreateQuizOptionAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("Correct Answer", result.Data!.Text);
            Assert.True(result.Data.IsCorrect);
        }

        [Fact]
        public async Task DeleteQuizOptionAsync_WhenExists_DeletesOption()
        {
            var context = await GetInMemoryDbContext();
            var question = new QuizQuestion { Title = "Parent Question", QuizId = 1 };
            context.QuizQuestions.Add(question);
            await context.SaveChangesAsync();

            var option = new QuizOption { Text = "To Delete", QuizQuestionId = question.Id };
            context.QuizOptions.Add(option);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteQuizOptionAsync(option.Id);

            Assert.True(result.Success, result.ErrorMessage);

            var dbOption = await context.QuizOptions.FindAsync(option.Id);
            Assert.Null(dbOption);
        }
    }
}