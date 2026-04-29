using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqeez.Api.Data;
using Sqeez.Api.DTOs;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Academics;
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

        private QuizOptionService CreateService(SqeezDbContext context, Mock<IMediaAssetService>? mockMediaAssetService = null)
        {
            var mockLogger = new Mock<ILogger<QuizOptionService>>();
            mockMediaAssetService ??= new Mock<IMediaAssetService>();

            return new QuizOptionService(context, mockLogger.Object, mockMediaAssetService.Object);
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
        public async Task GetAllQuizOptionsAsync_WhenTeacher_ReturnsOnlyTheirOptions()
        {
            var context = await GetInMemoryDbContext();
            long currentTeacherId = 1;
            long otherTeacherId = 99;

            var mySubject = CreateActiveSubject(currentTeacherId);
            var myQuiz = new Quiz { Subject = mySubject };
            var myQuestion = new QuizQuestion { Quiz = myQuiz };
            var myOption = new QuizOption { Text = "My Option", QuizQuestion = myQuestion };

            var otherSubject = CreateActiveSubject(otherTeacherId);
            var otherQuiz = new Quiz { Subject = otherSubject };
            var otherQuestion = new QuizQuestion { Quiz = otherQuiz };
            var otherOption = new QuizOption { Text = "Other Option", QuizQuestion = otherQuestion };

            context.Subjects.AddRange(mySubject, otherSubject);
            context.Quizzes.AddRange(myQuiz, otherQuiz);
            context.QuizQuestions.AddRange(myQuestion, otherQuestion);
            context.QuizOptions.AddRange(myOption, otherOption);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new QuizOptionFilterDto { PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllQuizOptionsAsync(filter, currentTeacherId, isAdmin: false);

            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.TotalCount);
            Assert.Equal("My Option", result.Data.Data.First().Text);
        }

        [Fact]
        public async Task GetAllQuizOptionsAsync_WhenAdmin_ReturnsAllOptions()
        {
            var context = await GetInMemoryDbContext();

            var subject1 = CreateActiveSubject(1);
            var subject2 = CreateActiveSubject(2);

            var quiz1 = new Quiz { Subject = subject1 };
            var quiz2 = new Quiz { Subject = subject2 };

            var question1 = new QuizQuestion { Quiz = quiz1 };
            var question2 = new QuizQuestion { Quiz = quiz2 };

            context.QuizOptions.Add(new QuizOption { QuizQuestion = question1 });
            context.QuizOptions.Add(new QuizOption { QuizQuestion = question2 });
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var filter = new QuizOptionFilterDto { PageNumber = 1, PageSize = 10 };

            var result = await service.GetAllQuizOptionsAsync(filter, 999, isAdmin: true);

            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.TotalCount);
        }

        [Fact]
        public async Task GetQuizOptionByIdAsync_WhenUnauthorizedTeacher_ReturnsForbidden()
        {
            var context = await GetInMemoryDbContext();
            long currentTeacherId = 1;
            long unauthorizedTeacherId = 99;

            var subject = CreateActiveSubject(currentTeacherId);
            var quiz = new Quiz { Subject = subject };
            var question = new QuizQuestion { Quiz = quiz };
            var option = new QuizOption { QuizQuestion = question };

            context.Subjects.Add(subject);
            context.Quizzes.Add(quiz);
            context.QuizQuestions.Add(question);
            context.QuizOptions.Add(option);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetQuizOptionByIdAsync(option.Id, unauthorizedTeacherId);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.Forbidden, result.ErrorCode);
        }

        [Fact]
        public async Task CreateQuizOptionAsync_WhenValidQuestion_CreatesOption()
        {
            var context = await GetInMemoryDbContext();
            long currentUserId = 1;

            var subject = CreateActiveSubject(currentUserId);
            var quiz = new Quiz { Subject = subject };
            var question = new QuizQuestion { Title = "Parent Question", Quiz = quiz };

            context.Subjects.Add(subject);
            context.Quizzes.Add(quiz);
            context.QuizQuestions.Add(question);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var dto = new CreateQuizOptionDto(
                true,
                question.Id,
                "Correct Answer",
                false,
                null
            );

            var result = await service.CreateQuizOptionAsync(dto, currentUserId);

            Assert.True(result.Success);
            Assert.Equal("Correct Answer", result.Data!.Text);
            Assert.True(result.Data.IsCorrect);
        }

        [Fact]
        public async Task CreateQuizOptionAsync_WhenMaxOptionsReached_ReturnsValidationFailed()
        {
            var context = await GetInMemoryDbContext();
            long currentUserId = 1;

            var subject = CreateActiveSubject(currentUserId);
            var quiz = new Quiz { Subject = subject };
            var question = new QuizQuestion { Quiz = quiz };

            for (int i = 0; i < Sqeez.Api.Constants.QuizConstants.MaxOptionsPerQuestion; i++)
            {
                question.Options.Add(new QuizOption { Text = $"Option {i}" });
            }

            context.Subjects.Add(subject);
            context.Quizzes.Add(quiz);
            context.QuizQuestions.Add(question);
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var dto = new CreateQuizOptionDto(false, question.Id, "One Too Many", false, null);

            var result = await service.CreateQuizOptionAsync(dto, currentUserId);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.ValidationFailed, result.ErrorCode);
            Assert.Contains("maximum", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteQuizOptionAsync_WhenExists_DeletesOption()
        {
            var context = await GetInMemoryDbContext();
            long currentUserId = 1;

            var subject = CreateActiveSubject(currentUserId);
            var quiz = new Quiz { Subject = subject };
            var question = new QuizQuestion { Title = "Parent Question", Quiz = quiz };
            var option = new QuizOption { Text = "To Delete", QuizQuestion = question };

            context.Subjects.Add(subject);
            context.Quizzes.Add(quiz);
            context.QuizQuestions.Add(question);
            context.QuizOptions.Add(option);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteQuizOptionAsync(option.Id, currentUserId, isAdmin: false);

            Assert.True(result.Success, result.ErrorMessage);

            var dbOption = await context.QuizOptions.FindAsync(option.Id);
            Assert.Null(dbOption);
        }

        [Fact]
        public async Task DeleteQuizOptionAsync_WhenSubjectHasEnded_ReturnsForbidden()
        {
            var context = await GetInMemoryDbContext();
            long currentUserId = 1;

            var subject = CreateEndedSubject(currentUserId);
            var quiz = new Quiz { Subject = subject };
            var question = new QuizQuestion { Quiz = quiz };
            var option = new QuizOption { QuizQuestion = question };

            context.Subjects.Add(subject);
            context.Quizzes.Add(quiz);
            context.QuizQuestions.Add(question);
            context.QuizOptions.Add(option);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.DeleteQuizOptionAsync(option.Id, currentUserId, isAdmin: false);

            Assert.False(result.Success);
            Assert.Equal(ServiceError.Forbidden, result.ErrorCode);
            Assert.Contains("already ended", result.ErrorMessage);
        }

        [Fact]
        public async Task PatchQuizOptionAsync_WhenValid_UpdatesFields()
        {
            var context = await GetInMemoryDbContext();
            long currentUserId = 1;

            var subject = CreateActiveSubject(currentUserId);
            var quiz = new Quiz { Subject = subject };
            var question = new QuizQuestion { Quiz = quiz };

            var option = new QuizOption
            {
                Text = "Old Text",
                IsCorrect = false,
                QuizQuestion = question
            };

            context.Subjects.Add(subject);
            context.Quizzes.Add(quiz);
            context.QuizQuestions.Add(question);
            context.QuizOptions.Add(option);
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var patchDto = new PatchQuizOptionDto("Updated Text", null, true, null);

            var result = await service.PatchQuizOptionAsync(option.Id, patchDto, currentUserId);

            Assert.True(result.Success);

            Assert.Equal("Updated Text", result.Data!.Text);
            Assert.True(result.Data.IsCorrect);

            var dbOption = await context.QuizOptions.FindAsync(option.Id);
            Assert.Equal("Updated Text", dbOption!.Text);
            Assert.True(dbOption.IsCorrect);
        }
    }
}