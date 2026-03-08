using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Academics;
using Sqeez.Api.Models.Users;
using BC = BCrypt.Net.BCrypt;

namespace Sqeez.Api.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(SqeezDbContext context, IConfiguration config)
        {
            // Check if we already have data. If yes, exit.
            if (await context.Students.AnyAsync())
            {
                return;
            }

            string superEmail = config["SUPER_USER_EMAIL"] ?? "test@example.com";
            string superPassword = config["SUPER_USER_DEFAULT_PASSWORD"] ?? "YourSuperSecretPassword123!";

            string salt = BC.GenerateSalt(12);
            string defaultPassword = BC.HashPassword("Heslo1122*", salt);
            string superPasswordHash = BC.HashPassword(superPassword, salt);

            var superAdmin = new Admin
            {
                Username = superEmail.Split('@')[0],
                Email = superEmail,
                PasswordHash = superPasswordHash,
                Role = UserRole.Admin,
                LastSeen = DateTime.UtcNow,
                Department = "Board",
                PhoneNumber = "00420123456789"
            };

            var mathClass = new SchoolClass
            {
                Name = "3.B",
                AcademicYear = "2025-2026",
                Section = "B"
            };

            context.SchoolClasses.Add(mathClass);

            var teacher = new Teacher
            {
                Username = "teacher_denda",
                Email = "denda@sqeez.com",
                PasswordHash = defaultPassword,
                Role = UserRole.Teacher,
                LastSeen = DateTime.UtcNow,
                Department = "Mathematics",
                ManagedClass = mathClass
            };

            var student = new Student
            {
                Username = "student_tonda",
                Email = "tonda@sqeez.com",
                PasswordHash = defaultPassword,
                Role = UserRole.Student,
                LastSeen = DateTime.UtcNow,
                SchoolClass = mathClass
            };

            context.Students.AddRange(superAdmin, teacher, student);

            await context.SaveChangesAsync();
        }
    }
}