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
    public class QuizAttemptServiceTests
    {
        private readonly DbContextOptions<SqeezDbContext> _dbContextOptions;
        private readonly Mock<ILogger<QuizAttemptService>> _mockLogger;

        public QuizAttemptServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<SqeezDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockLogger = new Mock<ILogger<QuizAttemptService>>();
        }

        private async Task<SqeezDbContext> GetSeededContextAsync()
        {
            var context = new SqeezDbContext(_dbContextOptions);

            var student = new Student { Id = 1, Username = "teststudent", Email = "test@test.com", PasswordHash = "hash" };
            var subject = new Subject { Id = 1, Name = "Math", Code = "M1" };
            var enrollment = new Enrollment { Id = 1, StudentId = 1, SubjectId = 1 };

            var quiz = new Quiz { Id = 1, SubjectId = 1, Title = "Test Quiz", MaxRetries = 1 };

            var question1 = new QuizQuestion { Id = 1, QuizId = 1, Difficulty = 5, Title = "MCQ" };
            var opt1 = new QuizOption { Id = 1, QuizQuestionId = 1, IsCorrect = true, Text = "A" };
            var opt2 = new QuizOption { Id = 2, QuizQuestionId = 1, IsCorrect = true, Text = "B" };
            var opt3 = new QuizOption { Id = 3, QuizQuestionId = 1, IsCorrect = false, Text = "C" };

            var question2 = new QuizQuestion { Id = 2, QuizId = 1, Difficulty = 10, Title = "FreeText" };
            var opt4 = new QuizOption { Id = 4, QuizQuestionId = 2, IsCorrect = true, IsFreeText = true, Text = "Earth" };

            context.Students.Add(student);
            context.Subjects.Add(subject);
            context.Enrollments.Add(enrollment);
            context.Quizzes.Add(quiz);
            context.QuizQuestions.AddRange(question1, question2);
            context.QuizOptions.AddRange(opt1, opt2, opt3, opt4);

            await context.SaveChangesAsync();
            return context;
        }

        [Fact]
        public async Task StartAttemptAsync_WhenValid_CreatesAttemptAndReturnsFirstQuestionId()
        {
            await using var context = await GetSeededContextAsync();
            var service = new QuizAttemptService(context, _mockLogger.Object);
            var dto = new StartQuizAttemptDto(QuizId: 1, EnrollmentId: 1);

            var result = await service.StartAttemptAsync(1, dto);

            Assert.True(result.Success);
            Assert.Equal(AttemptStatus.Created, result.Data!.Status);
            Assert.Equal(0, result.Data.TotalScore);
            Assert.Equal(1, await context.QuizAttempts.CountAsync());

            Assert.NotNull(result.Data.NextQuestionId);
            Assert.Equal(1, result.Data.NextQuestionId.Value);
        }

        [Fact]
        public async Task StartAttemptAsync_WhenMaxRetriesReached_ReturnsConflict()
        {
            await using var context = await GetSeededContextAsync();

            context.QuizAttempts.Add(new QuizAttempt { QuizId = 1, EnrollmentId = 1 });
            await context.SaveChangesAsync();

            var service = new QuizAttemptService(context, _mockLogger.Object);
            var dto = new StartQuizAttemptDto(QuizId: 1, EnrollmentId: 1);

            var result = await service.StartAttemptAsync(1, dto);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.Conflict, result.ErrorCode);
            Assert.Contains("maximum of 1 retries", result.ErrorMessage);
        }

        [Fact]
        public async Task SubmitAnswerAsync_MultipleChoice_PerfectMatch_AwardsFullPoints()
        {
            await using var context = await GetSeededContextAsync();
            var attempt = new QuizAttempt { Id = 1, QuizId = 1, EnrollmentId = 1, Status = AttemptStatus.Created };
            context.QuizAttempts.Add(attempt);
            await context.SaveChangesAsync();

            var service = new QuizAttemptService(context, _mockLogger.Object);

            var dto = new SubmitQuestionResponseDto(QuizQuestionId: 1, ResponseTimeMs: 5000, null, new List<long> { 1, 2 });

            var result = await service.SubmitAnswerAsync(1, 1, dto);

            Assert.True(result.Success);
            Assert.Equal(5, result.Data!.Score);

            Assert.Equal(2, result.Data.NextQuestionId);
        }

        [Fact]
        public async Task SubmitAnswerAsync_MultipleChoice_PartialMatch_AwardsZeroPoints()
        {
            await using var context = await GetSeededContextAsync();
            var attempt = new QuizAttempt { Id = 1, QuizId = 1, EnrollmentId = 1, Status = AttemptStatus.Created };
            context.QuizAttempts.Add(attempt);
            await context.SaveChangesAsync();

            var service = new QuizAttemptService(context, _mockLogger.Object);

            var dto = new SubmitQuestionResponseDto(QuizQuestionId: 1, ResponseTimeMs: 5000, null, new List<long> { 1, 3 });

            var result = await service.SubmitAnswerAsync(1, 1, dto);

            Assert.True(result.Success);
            Assert.Equal(0, result.Data!.Score);
        }

        [Fact]
        public async Task SubmitAnswerAsync_FreeText_LeavesScoreAtZeroForManualGrading()
        {
            await using var context = await GetSeededContextAsync();
            var attempt = new QuizAttempt { Id = 1, QuizId = 1, EnrollmentId = 1, Status = AttemptStatus.Created };
            context.QuizAttempts.Add(attempt);
            await context.SaveChangesAsync();

            var service = new QuizAttemptService(context, _mockLogger.Object);

            var dto = new SubmitQuestionResponseDto(QuizQuestionId: 2, ResponseTimeMs: 5000, "earth", new List<long>());

            var result = await service.SubmitAnswerAsync(1, 1, dto);

            Assert.True(result.Success);
            Assert.Equal(0, result.Data!.Score);

            Assert.Null(result.Data.NextQuestionId);
        }

        [Fact]
        public async Task SubmitAnswerAsync_AlreadyAnswered_ReturnsConflict()
        {
            await using var context = await GetSeededContextAsync();
            var attempt = new QuizAttempt { Id = 1, QuizId = 1, EnrollmentId = 1, Status = AttemptStatus.Started };

            var existingResponse = new QuizQuestionResponse { QuizAttemptId = 1, QuizQuestionId = 1 };

            context.QuizAttempts.Add(attempt);
            context.QuizQuestionResponses.Add(existingResponse);
            await context.SaveChangesAsync();

            var service = new QuizAttemptService(context, _mockLogger.Object);
            var dto = new SubmitQuestionResponseDto(QuizQuestionId: 1, ResponseTimeMs: 5000, null, new List<long> { 1, 2 });

            var result = await service.SubmitAnswerAsync(1, 1, dto);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.Conflict, result.ErrorCode);
            Assert.Contains("already submitted", result.ErrorMessage);
        }

        [Fact]
        public async Task CompleteAttemptAsync_CalculatesFinalScoreCorrectly()
        {
            await using var context = await GetSeededContextAsync();

            var attempt = new QuizAttempt
            {
                QuizId = 1,
                EnrollmentId = 1,
                Status = AttemptStatus.Started,
                Responses = new List<QuizQuestionResponse>
                {
                    new QuizQuestionResponse { QuizQuestionId = 1, Score = 5 },
                    new QuizQuestionResponse { QuizQuestionId = 2, Score = 10 }
                }
            };

            context.QuizAttempts.Add(attempt);
            await context.SaveChangesAsync();

            var service = new QuizAttemptService(context, _mockLogger.Object);

            var result = await service.CompleteAttemptAsync(attempt.Id, 1);

            Assert.True(result.Success, result.ErrorMessage);
            Assert.Equal(AttemptStatus.Completed, result.Data!.Status);
            Assert.Equal(15, result.Data.TotalScore);
            Assert.NotNull(result.Data.EndTime);
        }

        [Fact]
        public async Task GetNextPendingQuestionIdAsync_WhenNoAnswers_ReturnsFirstQuestionId()
        {
            await using var context = await GetSeededContextAsync();

            // Create a brand new attempt with NO responses yet
            var attempt = new QuizAttempt { Id = 1, QuizId = 1, EnrollmentId = 1, Status = AttemptStatus.Started };
            context.QuizAttempts.Add(attempt);
            await context.SaveChangesAsync();

            var service = new QuizAttemptService(context, _mockLogger.Object);

            var result = await service.GetNextPendingQuestionIdAsync(attempt.Id, 1);

            Assert.True(result.Success);
            Assert.Equal(1, result.Data); // Should point to Question 1
        }

        [Fact]
        public async Task GetNextPendingQuestionIdAsync_WhenPartiallyAnswered_ReturnsNextQuestionId()
        {
            await using var context = await GetSeededContextAsync();

            // Create an attempt where the student has only answered Question 1
            var attempt = new QuizAttempt
            {
                Id = 1,
                QuizId = 1,
                EnrollmentId = 1,
                Status = AttemptStatus.Started,
                Responses = new List<QuizQuestionResponse>
                {
                    new QuizQuestionResponse { QuizQuestionId = 1 }
                }
            };
            context.QuizAttempts.Add(attempt);
            await context.SaveChangesAsync();

            var service = new QuizAttemptService(context, _mockLogger.Object);

            var result = await service.GetNextPendingQuestionIdAsync(attempt.Id, 1);

            Assert.True(result.Success);
            Assert.Equal(2, result.Data); // Should perfectly resume at Question 2
        }

        [Fact]
        public async Task GetNextPendingQuestionIdAsync_WhenFullyAnswered_ReturnsNull()
        {
            await using var context = await GetSeededContextAsync();

            // Create an attempt where the student has answered BOTH questions
            var attempt = new QuizAttempt
            {
                Id = 1,
                QuizId = 1,
                EnrollmentId = 1,
                Status = AttemptStatus.Started,
                Responses = new List<QuizQuestionResponse>
                {
                    new QuizQuestionResponse { QuizQuestionId = 1 },
                    new QuizQuestionResponse { QuizQuestionId = 2 }
                }
            };
            context.QuizAttempts.Add(attempt);
            await context.SaveChangesAsync();

            var service = new QuizAttemptService(context, _mockLogger.Object);

            var result = await service.GetNextPendingQuestionIdAsync(attempt.Id, 1);

            Assert.True(result.Success);
            Assert.Null(result.Data); // No questions left!
        }

        [Fact]
        public async Task GetNextPendingQuestionIdAsync_WhenAttemptIsCompleted_ReturnsConflict()
        {
            await using var context = await GetSeededContextAsync();

            // Create an attempt that is already COMPLETED
            var attempt = new QuizAttempt { Id = 1, QuizId = 1, EnrollmentId = 1, Status = AttemptStatus.Completed };
            context.QuizAttempts.Add(attempt);
            await context.SaveChangesAsync();

            var service = new QuizAttemptService(context, _mockLogger.Object);

            var result = await service.GetNextPendingQuestionIdAsync(attempt.Id, 1);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.Conflict, result.ErrorCode);
            Assert.Contains("no longer in progress", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteAttemptAsync_WhenValid_DeletesAttemptAndResponses()
        {
            await using var context = await GetSeededContextAsync();

            var subject = await context.Subjects.FirstAsync();
            subject.TeacherId = 99;

            var attempt = new QuizAttempt
            {
                Id = 1,
                QuizId = 1,
                EnrollmentId = 1,
                Status = AttemptStatus.Started,
                Responses = new List<QuizQuestionResponse>
                {
                    new QuizQuestionResponse { QuizQuestionId = 1, Score = 5 }
                }
            };
            context.QuizAttempts.Add(attempt);
            await context.SaveChangesAsync();

            var service = new QuizAttemptService(context, _mockLogger.Object);

            var result = await service.DeleteAttemptAsync(attempt.Id, 99);

            Assert.True(result.Success);
            Assert.Equal(0, await context.QuizAttempts.CountAsync());
            Assert.Equal(0, await context.QuizQuestionResponses.CountAsync()); // Proves Cascade Delete works
        }

        [Fact]
        public async Task DeleteAttemptAsync_WhenAttemptNotFound_ReturnsNotFound()
        {
            await using var context = await GetSeededContextAsync();
            var service = new QuizAttemptService(context, _mockLogger.Object);

            var result = await service.DeleteAttemptAsync(9999, 1);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
        }

        [Fact]
        public async Task DeleteAttemptAsync_WhenWrongTeacher_ReturnsForbidden()
        {
            await using var context = await GetSeededContextAsync();

            var subject = await context.Subjects.FirstAsync();
            subject.TeacherId = 99;

            var attempt = new QuizAttempt { Id = 1, QuizId = 1, EnrollmentId = 1, Status = AttemptStatus.Completed };
            context.QuizAttempts.Add(attempt);
            await context.SaveChangesAsync();

            var service = new QuizAttemptService(context, _mockLogger.Object);

            var result = await service.DeleteAttemptAsync(attempt.Id, 42);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.Forbidden, result.ErrorCode);
            Assert.Contains("permission", result.ErrorMessage);

            Assert.Equal(1, await context.QuizAttempts.CountAsync());
        }
    }
}