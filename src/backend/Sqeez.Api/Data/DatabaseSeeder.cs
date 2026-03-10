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

            // --- 1. Admin ---
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

            // --- 2. School Classes ---
            var class3B = new SchoolClass
            {
                Name = "3.B",
                AcademicYear = "2025-2026",
                Section = "B"
            };

            var class3A = new SchoolClass
            {
                Name = "3.A",
                AcademicYear = "2025-2026",
                Section = "A"
            };

            context.SchoolClasses.AddRange(class3B, class3A);

            // --- 3. Teachers ---
            var teacherDenda = new Teacher
            {
                Username = "teacher_denda",
                Email = "denda@sqeez.com",
                PasswordHash = defaultPassword,
                Role = UserRole.Teacher,
                LastSeen = DateTime.UtcNow,
                Department = "Mathematics & Sciences",
                ManagedClass = class3B
            };

            var teacherJana = new Teacher
            {
                Username = "teacher_jana",
                Email = "jana@sqeez.com",
                PasswordHash = defaultPassword,
                Role = UserRole.Teacher,
                LastSeen = DateTime.UtcNow,
                Department = "Languages",
                ManagedClass = class3A
            };

            // --- 4. Students ---
            var studentTonda = new Student
            {
                Username = "student_tonda",
                Email = "tonda@sqeez.com",
                PasswordHash = defaultPassword,
                Role = UserRole.Student,
                LastSeen = DateTime.UtcNow,
                SchoolClass = class3B
            };

            var studentPepa = new Student
            {
                Username = "student_pepa",
                Email = "pepa@sqeez.com",
                PasswordHash = defaultPassword,
                Role = UserRole.Student,
                LastSeen = DateTime.UtcNow,
                SchoolClass = class3B
            };

            var studentKarel = new Student
            {
                Username = "student_karel",
                Email = "karel@sqeez.com",
                PasswordHash = defaultPassword,
                Role = UserRole.Student,
                LastSeen = DateTime.UtcNow,
                SchoolClass = class3A
            };

            var studentEva = new Student
            {
                Username = "student_eva",
                Email = "eva@sqeez.com",
                PasswordHash = defaultPassword,
                Role = UserRole.Student,
                LastSeen = DateTime.UtcNow,
                SchoolClass = class3A
            };

            context.Students.AddRange(superAdmin, teacherDenda, teacherJana, studentTonda, studentPepa, studentKarel, studentEva);

            // --- 5. Subjects ---
            var mathSubject = new Subject
            {
                Name = "Advanced Mathematics",
                Code = "MATH-3B",
                Description = "Calculus, Algebra, and Geometry",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(10),
                Teacher = teacherDenda,
                SchoolClass = class3B
            };

            var physicsSubject = new Subject
            {
                Name = "Physics",
                Code = "PHYS-3B",
                Description = "Mechanics and Thermodynamics",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(10),
                Teacher = teacherDenda,
                SchoolClass = class3B
            };

            var englishSubject = new Subject
            {
                Name = "English Literature",
                Code = "ENGL-3A",
                Description = "Shakespeare to Modern Era",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(10),
                Teacher = teacherJana,
                SchoolClass = class3A
            };

            context.Subjects.AddRange(mathSubject, physicsSubject, englishSubject);

            // --- 6. Enrollments ---
            var enrollments = new List<Enrollment>
            {
                // Tonda takes Math and Physics
                new Enrollment { Student = studentTonda, Subject = mathSubject, EnrolledAt = DateTime.UtcNow },
                new Enrollment { Student = studentTonda, Subject = physicsSubject, EnrolledAt = DateTime.UtcNow },
                
                // Pepa takes Math
                new Enrollment { Student = studentPepa, Subject = mathSubject, EnrolledAt = DateTime.UtcNow },
                
                // Karel takes English
                new Enrollment { Student = studentKarel, Subject = englishSubject, EnrolledAt = DateTime.UtcNow },
                
                // Eva takes English, but also an elective Math from 3.B
                new Enrollment { Student = studentEva, Subject = englishSubject, EnrolledAt = DateTime.UtcNow },
                new Enrollment { Student = studentEva, Subject = mathSubject, EnrolledAt = DateTime.UtcNow }
            };

            context.Enrollments.AddRange(enrollments);

            // Save everything to the database
            await context.SaveChangesAsync();
        }
    }
}