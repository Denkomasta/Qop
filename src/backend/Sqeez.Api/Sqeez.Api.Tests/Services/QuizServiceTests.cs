using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Academics;
using Sqeez.Api.Models.QuizSystem;
using Sqeez.Api.Services;

namespace Sqeez.Api.Tests.Services
{
    public class QuizServiceTests
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

        private QuizService CreateService(SqeezDbContext context)
        {
            var mockLogger = new Mock<ILogger<QuizService>>();
            return new QuizService(context, mockLogger.Object);
        }

        [Fact]
        public async Task GetQuizByIdAsync_WhenExists_ReturnsQuizDto()
        {
            var context = await GetInMemoryDbContext();
            var quiz = new Quiz { Title = "Math Quiz", Description = "Test", MaxRetries = 2, SubjectId = 1 };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetQuizByIdAsync(quiz.Id);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Math Quiz", result.Data.Title);
            Assert.Equal(2, result.Data.MaxRetries);
        }

        [Fact]
        public async Task GetAllQuizzesAsync_WithSubjectFilter_ReturnsFilteredResults()
        {
            var context = await GetInMemoryDbContext();

            context.Quizzes.AddRange(
                new Quiz { Title = "Math 1", SubjectId = 1 },
                new Quiz { Title = "Math 2", SubjectId = 1 },
                new Quiz { Title = "English 1", SubjectId = 2 }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var filter = new QuizFilterDto { SubjectId = 1 };
            var result = await service.GetAllQuizzesAsync(filter);

            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.TotalCount);
            Assert.All(result.Data.Data, q => Assert.Equal(1, q.SubjectId));
        }

        [Fact]
        public async Task CreateQuizAsync_WhenValidSubject_CreatesQuiz()
        {
            var context = await GetInMemoryDbContext();
            var subject = new Subject { Name = "Math", Code = "M1" };
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new CreateQuizDto("New Quiz", "Desc", subject.Id, 3);

            var result = await service.CreateQuizAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("New Quiz", result.Data!.Title);

            var dbQuiz = await context.Quizzes.FindAsync(result.Data.Id);
            Assert.NotNull(dbQuiz);
            Assert.Equal(subject.Id, dbQuiz.SubjectId);
        }

        [Fact]
        public async Task CreateQuizAsync_WhenInvalidSubject_ReturnsNotFound()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);

            var dto = new CreateQuizDto("Bad Quiz", "Desc", 999); // 999 does not exist

            var result = await service.CreateQuizAsync(dto);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
        }

        [Fact]
        public async Task DeleteQuizAsync_WhenNoAttempts_HardDeletesQuiz()
        {
            var context = await GetInMemoryDbContext();
            var quiz = new Quiz { Title = "To Be Deleted", SubjectId = 1 };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteQuizAsync(quiz.Id);

            Assert.True(result.Success);
            var dbQuiz = await context.Quizzes.FindAsync(quiz.Id);
            Assert.Null(dbQuiz);
        }
    }
}