using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Academics;
using Sqeez.Api.Models.QuizSystem;
using Sqeez.Api.Models.Users;
using Sqeez.Api.Services;
using Xunit;

namespace Sqeez.Api.Tests.Services
{
    public class QuizStatisticsServiceTests
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

        private QuizStatisticsService CreateService(SqeezDbContext context)
        {
            var mockLogger = new Mock<ILogger<QuizStatisticsService>>();
            return new QuizStatisticsService(context, mockLogger.Object);
        }

        [Fact]
        public async Task GetQuizSummaryStatsAsync_WhenQuizNotFound_ReturnsFailure()
        {
            var context = await GetInMemoryDbContext();
            var service = CreateService(context);

            var result = await service.GetQuizSummaryStatsAsync(999, 1);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
        }

        [Fact]
        public async Task GetQuizSummaryStatsAsync_WhenTeacherDoesNotOwnQuiz_ReturnsForbidden()
        {
            var context = await GetInMemoryDbContext();
            var teacher1 = new Teacher { Id = 1, Username = "teacher1", Email = "t1@sqeez.org" };
            var subject = new Subject { Id = 1, TeacherId = teacher1.Id, Name = "Math", Code = "M1" };
            var quiz = new Quiz { Id = 1, SubjectId = subject.Id, Title = "Quiz 1", PublishDate = DateTime.UtcNow, ClosingDate = DateTime.UtcNow.AddDays(1) };
            
            context.Teachers.Add(teacher1);
            context.Subjects.Add(subject);
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetQuizSummaryStatsAsync(quiz.Id, 999);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.Forbidden, result.ErrorCode);
        }

        [Fact]
        public async Task GetQuizSummaryStatsAsync_WithCompletedAttempts_ReturnsCorrectStats()
        {
            var context = await GetInMemoryDbContext();
            var teacher = new Teacher { Id = 1, Username = "teacher", Email = "t@sqeez.org" };
            var subject = new Subject { Id = 1, TeacherId = teacher.Id, Name = "Math", Code = "M1" };
            var quiz = new Quiz { Id = 1, SubjectId = subject.Id, Title = "Quiz 1", PublishDate = new DateTime(2024,1,1), ClosingDate = new DateTime(2024,2,1) };
            
            var attempt1 = new QuizAttempt { Id = 1, QuizId = quiz.Id, Status = AttemptStatus.Completed, TotalScore = 80, StartTime = new DateTime(2024,1,1,10,0,0), EndTime = new DateTime(2024,1,1,10,10,0) };
            var attempt2 = new QuizAttempt { Id = 2, QuizId = quiz.Id, Status = AttemptStatus.Completed, TotalScore = 100, StartTime = new DateTime(2024,1,1,11,0,0), EndTime = new DateTime(2024,1,1,11,15,0) };
            var attempt3 = new QuizAttempt { Id = 3, QuizId = quiz.Id, Status = AttemptStatus.Started, TotalScore = 0 };
            
            context.Teachers.Add(teacher);
            context.Subjects.Add(subject);
            context.Quizzes.Add(quiz);
            context.QuizAttempts.AddRange(attempt1, attempt2, attempt3);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetQuizSummaryStatsAsync(quiz.Id, teacher.Id);

            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.TotalAttempts);
            Assert.Equal(2, result.Data.CompletedAttempts);
            Assert.Equal(90, result.Data.AverageScore);
            Assert.Equal(100, result.Data.HighestScore);
            Assert.Equal(80, result.Data.LowestScore);
            Assert.Equal(12.5, result.Data.AverageCompletionTimeMinutes);
        }
        
        [Fact]
        public async Task GetQuestionStatsAsync_ReturnsDetailedQuestionStats()
        {
            var context = await GetInMemoryDbContext();
            var teacher = new Teacher { Id = 1, Username = "teacher", Email = "t@sqeez.org" };
            var subject = new Subject { Id = 1, TeacherId = teacher.Id, Name = "Math", Code = "M1" };
            var quiz = new Quiz { Id = 1, SubjectId = subject.Id, Title = "Quiz 1", PublishDate = DateTime.UtcNow, ClosingDate = DateTime.UtcNow.AddDays(1) };
            
            var question = new QuizQuestion { Id = 1, QuizId = quiz.Id, Title = "Q1", Difficulty = 1 };
            var option1 = new QuizOption { Id = 1, QuizQuestionId = question.Id, Text = "Opt1", IsCorrect = true };
            var option2 = new QuizOption { Id = 2, QuizQuestionId = question.Id, Text = "Opt2", IsCorrect = false };
            question.Options.Add(option1);
            question.Options.Add(option2);

            var attempt = new QuizAttempt { Id = 1, QuizId = quiz.Id, Status = AttemptStatus.Completed, TotalScore = 10 };
            var response = new QuizQuestionResponse { Id = 1, QuizAttemptId = attempt.Id, QuizQuestionId = question.Id, Score = 10, ResponseTimeMs = 5000 };
            response.Options.Add(option1);
            
            // To make Many-to-Many work with InMemory db we might need to add to both
            option1.Responses.Add(response);

            context.Teachers.Add(teacher);
            context.Subjects.Add(subject);
            context.Quizzes.Add(quiz);
            context.QuizQuestions.Add(question);
            context.QuizAttempts.Add(attempt);
            context.QuizQuestionResponses.Add(response);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetQuestionStatsAsync(quiz.Id, teacher.Id);

            Assert.True(result.Success);
            Assert.Single(result.Data!);
            var stat = result.Data!.First();
            Assert.Equal(1, stat.TotalAnswers);
            Assert.Equal(10, stat.AverageScore);
            Assert.Equal(5, stat.AverageResponseTimeSeconds);
            Assert.Equal(2, stat.Options.Count);
            
            var opt1Stat = stat.Options.First(o => o.Id == option1.Id);
            Assert.Equal(1, opt1Stat.PickCount);
            
            var opt2Stat = stat.Options.First(o => o.Id == option2.Id);
            Assert.Equal(0, opt2Stat.PickCount);
        }
    }
}
