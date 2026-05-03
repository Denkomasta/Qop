using System.Net;
using Microsoft.AspNetCore.Http;
using Moq;
using Sqeez.Api.DTOs;
using Sqeez.Api.Models.Import;

namespace Sqeez.Api.Tests.Integration
{
    public class ImportControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public ImportControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _factory.ResetMocks();
        }

        [Fact]
        public async Task ImportMasterFile_AsAdmin_PassesUploadedFileToService()
        {
            _factory.CsvImportServiceMock
                .Setup(service => service.ImportMasterFileAsync(It.Is<IFormFile>(file =>
                    file.FileName == "master.csv" &&
                    file.Length > 0)))
                .ReturnsAsync(ServiceResult<ImportResultDto>.Ok(
                    new ImportResultDto { RecordsImported = 1 }));

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, "1");
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, "Admin");

            using var content = new MultipartFormDataContent();
            var fileContent = new StringContent("Class Name,First Name,Last Name,Email");
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
            content.Add(fileContent, "file", "master.csv");

            var response = await client.PostAsync("/api/import/master", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _factory.CsvImportServiceMock.Verify(
                service => service.ImportMasterFileAsync(It.IsAny<IFormFile>()),
                Times.Once);
        }

        [Fact]
        public async Task ImportMasterFile_AsTeacher_ReturnsForbiddenBeforeCallingService()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, "42");
            client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, "Teacher");

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent("x"), "file", "master.csv");

            var response = await client.PostAsync("/api/import/master", content);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            _factory.CsvImportServiceMock.Verify(
                service => service.ImportMasterFileAsync(It.IsAny<IFormFile>()),
                Times.Never);
        }
    }
}
