using System.Net;
using Moq;
using Sqeez.Api.DTOs;

namespace Sqeez.Api.Tests.Integration
{
    public class QuizStatisticsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public QuizStatisticsControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _factory.ResetMocks();
        }

        [Fact]
        public async Task GetQuizSummaryStats_AsTeacher_UsesQuizIdAndCurrentUserId()
        {
            _factory.QuizStatisticsServiceMock
                .Setup(service => service.GetQuizSummaryStatsAsync(5, 42))
                .ReturnsAsync(ServiceResult<QuizSummaryStatDto>.Ok(
                    new QuizSummaryStatDto
                    {
                        QuizId = 5,
                        TotalAttempts = 3,
                        CompletedAttempts = 2,
                        AverageScore = 8.5,
                        HighestScore = 10,
                        LowestScore = 7
                    }));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, "42");
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, "Teacher");

            var response = await client.GetAsync("/api/quizzes/5/statistics/summary");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _factory.QuizStatisticsServiceMock.Verify(
                service => service.GetQuizSummaryStatsAsync(5, 42),
                Times.Once);
        }

        [Fact]
        public async Task GetQuestionStats_AsStudent_ReturnsForbiddenBeforeCallingService()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, "7");
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, "Student");

            var response = await client.GetAsync("/api/quizzes/5/statistics/questions");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            _factory.QuizStatisticsServiceMock.Verify(
                service => service.GetQuestionStatsAsync(It.IsAny<long>(), It.IsAny<long>()),
                Times.Never);
        }
    }
}
