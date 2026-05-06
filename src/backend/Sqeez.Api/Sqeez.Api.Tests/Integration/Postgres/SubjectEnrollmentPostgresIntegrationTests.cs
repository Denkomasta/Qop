using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sqeez.Api.Data;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Academics;
using Sqeez.Api.Models.QuizSystem;
using Sqeez.Api.Models.Users;

namespace Sqeez.Api.Tests.Integration.Postgres
{
    [Collection(PostgresIntegrationTestCollection.CollectionName)]
    public class SubjectEnrollmentPostgresIntegrationTests
    {
        private readonly PostgresIntegrationTestFixture _fixture;

        public SubjectEnrollmentPostgresIntegrationTests(PostgresIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [DockerAvailableFact]
        public async Task CreateSubject_AsAdmin_PersistsSubjectWithTeacher()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedUsersAndSubjectAsync(createSubject: false);
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Admin);

            var response = await client.PostAsJsonAsync("/api/subjects", new
            {
                name = "Live created subject",
                code = PostgresTestHelpers.UniqueValue("created", 30),
                description = "Created through the live API.",
                teacherId = seed.Teacher.Id,
                startDate = DateTime.UtcNow.AddDays(-1)
            });

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, response);
            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var subjectId = json.RootElement.GetProperty("id").GetInt64();

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var subject = await dbContext.Subjects.SingleAsync(subject => subject.Id == subjectId);

            Assert.Equal("Live created subject", subject.Name);
            Assert.Equal(seed.Teacher.Id, subject.TeacherId);
            Assert.Null(subject.EndDate);
        }

        [DockerAvailableFact]
        public async Task CreateSubject_WithDescriptionAndDatesWithoutOffset_ReturnsBadRequest()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedUsersAndSubjectAsync(createSubject: false);
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Admin);

            var response = await client.PostAsJsonAsync("/api/subjects", new
            {
                name = "Live dated subject",
                code = PostgresTestHelpers.UniqueValue("dated", 30),
                description = "Description together with date-only frontend values.",
                teacherId = seed.Teacher.Id,
                startDate = "2026-01-15T08:30:00",
                endDate = "2026-06-15T16:00:00"
            });

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.BadRequest, response);

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();

            Assert.False(await dbContext.Subjects.AnyAsync());
        }

        [DockerAvailableFact]
        public async Task CreateSubject_WithDescriptionAndUtcDates_PersistsSubject()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedUsersAndSubjectAsync(createSubject: false);
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Admin);

            var response = await client.PostAsJsonAsync("/api/subjects", new
            {
                name = "Live UTC subject",
                code = PostgresTestHelpers.UniqueValue("utc", 30),
                description = "Description together with UTC date-time values.",
                teacherId = seed.Teacher.Id,
                startDate = "2026-01-15T08:30:00Z",
                endDate = "2026-06-15T16:00:00Z"
            });

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, response);
            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var subjectId = json.RootElement.GetProperty("id").GetInt64();

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var subject = await dbContext.Subjects.SingleAsync(subject => subject.Id == subjectId);

            Assert.Equal("Description together with UTC date-time values.", subject.Description);
            Assert.Equal(DateTimeKind.Utc, subject.StartDate.Kind);
            Assert.Equal(DateTimeKind.Utc, subject.EndDate!.Value.Kind);
            Assert.Equal(new DateTime(2026, 1, 15, 8, 30, 0, DateTimeKind.Utc), subject.StartDate);
            Assert.Equal(new DateTime(2026, 6, 15, 16, 0, 0, DateTimeKind.Utc), subject.EndDate.Value);
        }

        [DockerAvailableFact]
        public async Task GetSubjects_WithSearchTerm_ReturnsPersistedSubject()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedUsersAndSubjectAsync(subjectName: "Observable PostgreSQL Subject");
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Student);

            var response = await client.GetAsync("/api/subjects?searchTerm=Observable&pageSize=5");

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, response);
            var body = await response.Content.ReadAsStringAsync();

            Assert.Contains(seed.Subject!.Code, body);
            Assert.Contains("\"totalCount\":1", body);
        }

        [DockerAvailableFact]
        public async Task EnrollStudents_AsAdmin_PersistsNewEnrollmentsAndReportsDuplicateIds()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedUsersAndSubjectAsync();
            var secondStudent = await AddStudentAsync("second");
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Admin);

            var firstResponse = await client.PostAsJsonAsync($"/api/subjects/{seed.Subject!.Id}/enrollments", new
            {
                studentIds = new[] { seed.Student.Id, secondStudent.Id }
            });

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, firstResponse);

            var duplicateResponse = await client.PostAsJsonAsync($"/api/subjects/{seed.Subject.Id}/enrollments", new
            {
                studentIds = new[] { seed.Student.Id }
            });

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, duplicateResponse);
            using var json = await JsonDocument.ParseAsync(await duplicateResponse.Content.ReadAsStreamAsync());
            var duplicateId = json.RootElement.GetProperty("alreadyEnrolledIds")[0].GetInt64();

            Assert.Equal(seed.Student.Id, duplicateId);

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();

            Assert.Equal(2, await dbContext.Enrollments.CountAsync(enrollment => enrollment.SubjectId == seed.Subject.Id));
        }

        [DockerAvailableFact]
        public async Task EnrollStudents_AsStudentForSelf_CreatesEnrollment()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedUsersAndSubjectAsync();
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Student);

            var response = await client.PostAsJsonAsync($"/api/subjects/{seed.Subject!.Id}/enrollments", new
            {
                studentIds = new[] { seed.Student.Id }
            });

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, response);

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();

            Assert.True(await dbContext.Enrollments.AnyAsync(enrollment =>
                enrollment.SubjectId == seed.Subject.Id &&
                enrollment.StudentId == seed.Student.Id &&
                enrollment.ArchivedAt == null));
        }

        [DockerAvailableFact]
        public async Task UnenrollStudents_WithoutAttempts_HardDeletesEnrollment()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedUsersAndSubjectAsync(enrollStudent: true);
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Admin);

            using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/subjects/{seed.Subject!.Id}/enrollments")
            {
                Content = JsonContent.Create(new { studentIds = new[] { seed.Student.Id } })
            };

            var response = await client.SendAsync(request);

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, response);

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();

            Assert.False(await dbContext.Enrollments.AnyAsync(enrollment => enrollment.Id == seed.Enrollment!.Id));
        }

        [DockerAvailableFact]
        public async Task UnenrollStudents_WithAttempts_ArchivesEnrollment()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedUsersAndSubjectAsync(enrollStudent: true, createQuizAttempt: true);
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Admin);

            using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/subjects/{seed.Subject!.Id}/enrollments")
            {
                Content = JsonContent.Create(new { studentIds = new[] { seed.Student.Id } })
            };

            var response = await client.SendAsync(request);

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, response);

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var enrollment = await dbContext.Enrollments.SingleAsync(enrollment => enrollment.Id == seed.Enrollment!.Id);

            Assert.NotNull(enrollment.ArchivedAt);
        }

        [DockerAvailableFact]
        public async Task PatchEnrollment_AsSubjectTeacher_SetsAndRemovesMark()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedUsersAndSubjectAsync(enrollStudent: true);
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Teacher);

            var markResponse = await client.PatchAsJsonAsync($"/api/enrollments/{seed.Enrollment!.Id}", new
            {
                mark = 2
            });

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, markResponse);

            var removeResponse = await client.PatchAsJsonAsync($"/api/enrollments/{seed.Enrollment.Id}", new
            {
                removeMark = true
            });

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, removeResponse);

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var enrollment = await dbContext.Enrollments.SingleAsync(enrollment => enrollment.Id == seed.Enrollment.Id);

            Assert.Null(enrollment.Mark);
        }

        [DockerAvailableFact]
        public async Task DeleteEnrollment_AsStudentOwner_ArchivesEnrollmentWithAttemptHistory()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedUsersAndSubjectAsync(enrollStudent: true, createQuizAttempt: true);
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Student);

            var response = await client.DeleteAsync($"/api/enrollments/{seed.Enrollment!.Id}");

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, response);

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var enrollment = await dbContext.Enrollments.SingleAsync(enrollment => enrollment.Id == seed.Enrollment.Id);

            Assert.NotNull(enrollment.ArchivedAt);
        }

        [DockerAvailableFact]
        public async Task DeleteSubject_WithEnrollmentHistory_SoftDeletesSubject()
        {
            await _fixture.ResetDatabaseAsync();
            var seed = await SeedUsersAndSubjectAsync(enrollStudent: true);
            var client = PostgresTestHelpers.CreateAuthenticatedClient(_fixture, seed.Admin);

            var response = await client.DeleteAsync($"/api/subjects/{seed.Subject!.Id}");

            await PostgresTestHelpers.AssertStatusCodeAsync(HttpStatusCode.OK, response);

            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();
            var subject = await dbContext.Subjects.SingleAsync(subject => subject.Id == seed.Subject.Id);

            Assert.NotNull(subject.EndDate);
        }

        private async Task<SubjectEnrollmentSeed> SeedUsersAndSubjectAsync(
            bool createSubject = true,
            bool enrollStudent = false,
            bool createQuizAttempt = false,
            string subjectName = "Live subject")
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();

            var student = await PostgresTestHelpers.SeedStudentAsync(dbContext);
            var teacher = await PostgresTestHelpers.SeedTeacherAsync(dbContext);
            var admin = await PostgresTestHelpers.SeedAdminAsync(dbContext);

            Subject? subject = null;
            Enrollment? enrollment = null;
            Quiz? quiz = null;

            if (createSubject)
            {
                subject = new Subject
                {
                    Name = subjectName,
                    Code = PostgresTestHelpers.UniqueValue("subject", 30),
                    Description = "Live subject for PostgreSQL integration tests.",
                    StartDate = DateTime.UtcNow.AddDays(-7),
                    TeacherId = teacher.Id
                };

                dbContext.Subjects.Add(subject);
                await dbContext.SaveChangesAsync();
            }

            if (enrollStudent)
            {
                enrollment = new Enrollment
                {
                    StudentId = student.Id,
                    SubjectId = subject!.Id,
                    EnrolledAt = DateTime.UtcNow
                };

                dbContext.Enrollments.Add(enrollment);
                await dbContext.SaveChangesAsync();
            }

            if (createQuizAttempt)
            {
                quiz = new Quiz
                {
                    SubjectId = subject!.Id,
                    Title = "History quiz",
                    Description = "Creates attempt history.",
                    CreatedAt = DateTime.UtcNow,
                    PublishDate = DateTime.UtcNow.AddDays(-1)
                };

                dbContext.Quizzes.Add(quiz);
                await dbContext.SaveChangesAsync();

                dbContext.QuizAttempts.Add(new QuizAttempt
                {
                    QuizId = quiz.Id,
                    EnrollmentId = enrollment!.Id,
                    StartTime = DateTime.UtcNow,
                    Status = AttemptStatus.Completed,
                    TotalScore = 1,
                    EndTime = DateTime.UtcNow
                });
                await dbContext.SaveChangesAsync();
            }

            return new SubjectEnrollmentSeed(student, teacher, admin, subject, enrollment, quiz);
        }

        private async Task<Student> AddStudentAsync(string prefix)
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SqeezDbContext>();

            return await PostgresTestHelpers.SeedStudentAsync(dbContext, prefix);
        }

        private sealed record SubjectEnrollmentSeed(
            Student Student,
            Teacher Teacher,
            Admin Admin,
            Subject? Subject,
            Enrollment? Enrollment,
            Quiz? Quiz);
    }
}
