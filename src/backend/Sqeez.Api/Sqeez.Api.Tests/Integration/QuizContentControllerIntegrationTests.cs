using System.Net;
using System.Net.Http.Json;
using Moq;
using Sqeez.Api.DTOs;

namespace Sqeez.Api.Tests.Integration
{
    public class QuizContentControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public QuizContentControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _factory.ResetMocks();
        }

        [Fact]
        public async Task CreateQuestion_UsesQuizIdFromRoute()
        {
            _factory.QuizQuestionServiceMock
                .Setup(service => service.CreateQuizQuestionAsync(
                    It.Is<CreateQuizQuestionDto>(dto => dto.QuizId == 9 && dto.Title == "Question"),
                    42))
                .ReturnsAsync(ServiceResult<QuizQuestionDto>.Ok(
                    new QuizQuestionDto(3, "Question", 10, false, 60, false, 9, null, 0, 0)));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, "42");
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, "Teacher");

            var response = await client.PostAsJsonAsync("/api/quizzes/9/questions", new
            {
                title = "Question",
                difficulty = 10,
                timeLimit = 60,
                quizId = 999
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _factory.QuizQuestionServiceMock.Verify(
                service => service.CreateQuizQuestionAsync(It.IsAny<CreateQuizQuestionDto>(), It.IsAny<long>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateOption_UsesQuestionIdFromRoute()
        {
            _factory.QuizOptionServiceMock
                .Setup(service => service.CreateQuizOptionAsync(
                    It.Is<CreateQuizOptionDto>(dto => dto.QuizQuestionID == 3 && dto.Text == "Answer"),
                    42))
                .ReturnsAsync(ServiceResult<QuizOptionDto>.Ok(
                    new QuizOptionDto(4, "Answer", false, true, 3, null, 0)));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, "42");
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, "Teacher");

            var response = await client.PostAsJsonAsync("/api/quizzes/9/questions/3/options", new
            {
                text = "Answer",
                isCorrect = true,
                quizQuestionID = 999
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _factory.QuizOptionServiceMock.Verify(
                service => service.CreateQuizOptionAsync(It.IsAny<CreateQuizOptionDto>(), It.IsAny<long>()),
                Times.Once);
        }
    }
}
