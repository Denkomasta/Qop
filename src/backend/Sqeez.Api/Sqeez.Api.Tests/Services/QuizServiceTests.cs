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

        private Subject CreateActiveSubject(long teacherId)
        {
            return new Subject
            {
                TeacherId = teacherId,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(30)
            };
        }

        private Subject CreateEndedSubject(long teacherId)
        {
            return new Subject
            {
                TeacherId = teacherId,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(-1)
            };
        }

        [Fact]
        public async Task GetQuizByIdAsync_WhenExists_ReturnsQuizDto()
        {
            var context = await GetInMemoryDbContext();
            var quiz = new Quiz { Title = "Math Quiz", Description = "Test", MaxRetries = 2, SubjectId = 1 };
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new GetQuizDto(null);

            var result = await service.GetQuizByIdAsync(quiz.Id, dto);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Math Quiz", result.Data.Title);
            Assert.Equal(2, result.Data.MaxRetries);
        }

        [Fact]
        public async Task CreateQuizAsync_WhenValidSubject_CreatesQuiz()
        {
            var context = await GetInMemoryDbContext();
            long currentUserId = 1;

            var subject = CreateActiveSubject(currentUserId);
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var dto = new CreateQuizDto("New Quiz", "Desc", subject.Id, 3, null, null);

            var result = await service.CreateQuizAsync(dto, currentUserId);

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
            long currentUserId = 1;
            var service = CreateService(context);

            var dto = new CreateQuizDto("Bad Quiz", "Desc", 999, 3, null, null); // 999 does not exist

            var result = await service.CreateQuizAsync(dto, currentUserId);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.NotFound, result.ErrorCode);
        }

        [Fact]
        public async Task CreateQuizAsync_WhenClosingDateExceedsSubjectEndDate_ReturnsValidationFailed()
        {
            var context = await GetInMemoryDbContext();
            long currentUserId = 1;

            var subject = CreateActiveSubject(currentUserId);
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            DateTime badClosingDate = subject.EndDate!.Value.AddDays(5);
            var dto = new CreateQuizDto("New Quiz", "Desc", subject.Id, 3, null, badClosingDate);

            var result = await service.CreateQuizAsync(dto, currentUserId);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.ValidationFailed, result.ErrorCode);
            Assert.Contains("cannot be later than the subject's end date", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteQuizAsync_WhenNoAttempts_HardDeletesQuiz()
        {
            var context = await GetInMemoryDbContext();
            long currentUserId = 1;

            var subject = CreateActiveSubject(currentUserId);
            var quiz = new Quiz { Title = "To Be Deleted", Subject = subject };

            context.Subjects.Add(subject);
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteQuizAsync(quiz.Id, currentUserId, isAdmin: false);

            Assert.True(result.Success);
            var dbQuiz = await context.Quizzes.FindAsync(quiz.Id);
            Assert.Null(dbQuiz); // Verifies hard delete
        }

        [Fact]
        public async Task DeleteQuizAsync_WhenAttemptsExist_SoftDeletesQuiz()
        {
            var context = await GetInMemoryDbContext();
            long currentUserId = 1;
            long studentId = 123;

            var subject = CreateActiveSubject(currentUserId);
            var quiz = new Quiz { Title = "Soft Delete Me", Subject = subject };
            var enrollment = new Enrollment { Subject = subject, StudentId = studentId };
            var attempt = new QuizAttempt { Quiz = quiz, Enrollment = enrollment };

            context.Subjects.Add(subject);
            context.Enrollments.Add(enrollment);
            context.Quizzes.Add(quiz);
            context.QuizAttempts.Add(attempt);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteQuizAsync(quiz.Id, currentUserId, isAdmin: false);

            Assert.True(result.Success);

            // Verify soft delete (Quiz still exists, but ClosingDate is set)
            var dbQuiz = await context.Quizzes.FindAsync(quiz.Id);
            Assert.NotNull(dbQuiz);
            Assert.NotNull(dbQuiz.ClosingDate);
        }

        [Fact]
        public async Task DeleteQuizAsync_WhenSubjectHasEnded_ReturnsForbidden()
        {
            var context = await GetInMemoryDbContext();
            long currentUserId = 1;

            var subject = CreateEndedSubject(currentUserId);
            var quiz = new Quiz { Subject = subject };

            context.Subjects.Add(subject);
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteQuizAsync(quiz.Id, currentUserId, isAdmin: false);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.Forbidden, result.ErrorCode);
        }

        [Fact]
        public async Task PatchQuizAsync_WhenValidRequest_UpdatesQuiz()
        {
            var context = await GetInMemoryDbContext();
            long currentUserId = 1;

            var subject = CreateActiveSubject(currentUserId);
            var quiz = new Quiz { Title = "Old Title", Subject = subject, MaxRetries = 1 };
            context.Subjects.Add(subject);
            context.Quizzes.Add(quiz);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new PatchQuizDto(Title: "New Title", MaxRetries: 5);

            var result = await service.PatchQuizAsync(quiz.Id, dto, currentUserId);

            Assert.True(result.Success);
            Assert.Equal("New Title", result.Data!.Title);
            Assert.Equal(5, result.Data.MaxRetries);

            var dbQuiz = await context.Quizzes.FindAsync(quiz.Id);
            Assert.Equal("New Title", dbQuiz!.Title);
            Assert.Equal(5, dbQuiz.MaxRetries);
        }

        [Fact]
        public async Task GetAllQuizzesAsync_WithSubjectFilter_ReturnsFilteredQuizzes()
        {
            var context = await GetInMemoryDbContext();
            var subject1 = new Subject { Id = 1, TeacherId = 1, Name = "Subject 1" };
            var subject2 = new Subject { Id = 2, TeacherId = 1, Name = "Subject 2" };

            context.Subjects.AddRange(subject1, subject2);
            context.Quizzes.AddRange(
                new Quiz { Title = "Quiz 1", SubjectId = 1 },
                new Quiz { Title = "Quiz 2", SubjectId = 1 },
                new Quiz { Title = "Quiz 3", SubjectId = 2 }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new QuizFilterDto { SubjectId = 1, PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllQuizzesAsync(filter);

            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.TotalCount);
            Assert.All(result.Data.Data, q => Assert.Equal(1, q.SubjectId));
        }
    }
}