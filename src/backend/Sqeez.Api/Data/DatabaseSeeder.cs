using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Academics;
using Sqeez.Api.Models.Gamification; // <-- Added the Gamification namespace!
using Sqeez.Api.Models.Media;
using Sqeez.Api.Models.QuizSystem;
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
                FirstName = "System",
                LastName = "Master",
                Username = superEmail.Split('@')[0],
                Email = superEmail,
                PasswordHash = superPasswordHash,
                Role = UserRole.Admin,
                LastSeen = DateTime.UtcNow,
                Department = "Board",
                PhoneNumber = "00420123456789",
                IsEmailVerified = true
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
                FirstName = "Denda",
                LastName = "Valachu",
                Username = "teacher_denda",
                Email = "denda@sqeez.org",
                PasswordHash = defaultPassword,
                Role = UserRole.Teacher,
                LastSeen = DateTime.UtcNow,
                Department = "Mathematics & Sciences",
                ManagedClass = class3B,
                IsEmailVerified = true
            };

            var teacherJana = new Teacher
            {
                FirstName = "Jana",
                LastName = "Hrouzkova",
                Username = "teacher_jana",
                Email = "jana@sqeez.org",
                PasswordHash = defaultPassword,
                Role = UserRole.Teacher,
                LastSeen = DateTime.UtcNow,
                Department = "Languages",
                ManagedClass = class3A,
                IsEmailVerified = true
            };

            var avatarsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
            if (!Directory.Exists(avatarsFolder)) Directory.CreateDirectory(avatarsFolder);

            // A tiny grey 1x1 pixel PNG for the default avatar
            string base64AvatarPng = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";
            byte[] avatarBytes = Convert.FromBase64String(base64AvatarPng);

            if (!File.Exists(Path.Combine(avatarsFolder, "default-avatar.png")))
            {
                await File.WriteAllBytesAsync(Path.Combine(avatarsFolder, "default-avatar.png"), avatarBytes);
            }

            // --- 4. Students ---
            var studentTonda = new Student
            {
                FirstName = "Antonín",
                LastName = "Tučný",
                Username = "student_tonda",
                Email = "tonda@sqeez.org",
                PasswordHash = defaultPassword,
                Role = UserRole.Student,
                LastSeen = DateTime.UtcNow,
                SchoolClass = class3B,
                AvatarUrl = "/avatars/default-avatar.png",
                IsEmailVerified = true,
            };

            var studentPepa = new Student
            {
                FirstName = "Josef",
                LastName = "Nohavica",
                Username = "student_pepa",
                Email = "pepa@sqeez.org",
                PasswordHash = defaultPassword,
                Role = UserRole.Student,
                LastSeen = DateTime.UtcNow,
                SchoolClass = class3B,
                IsEmailVerified = true,
            };

            var studentKarel = new Student
            {
                FirstName = "Karel",
                LastName = "Eisenstadt",
                Username = "student_karel",
                Email = "karel@sqeez.org",
                PasswordHash = defaultPassword,
                Role = UserRole.Student,
                LastSeen = DateTime.UtcNow,
                SchoolClass = class3A,
                IsEmailVerified = true,
            };

            var studentEva = new Student
            {
                FirstName = "Eva",
                LastName = "Tomanová",
                Username = "student_eva",
                Email = "eva@sqeez.org",
                PasswordHash = defaultPassword,
                Role = UserRole.Student,
                LastSeen = DateTime.UtcNow,
                SchoolClass = class3A,
                IsEmailVerified = true,
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
                new Enrollment { Student = studentTonda, Subject = mathSubject, EnrolledAt = DateTime.UtcNow },
                new Enrollment { Student = studentTonda, Subject = physicsSubject, EnrolledAt = DateTime.UtcNow },
                new Enrollment { Student = studentPepa, Subject = mathSubject, EnrolledAt = DateTime.UtcNow },
                new Enrollment { Student = studentKarel, Subject = englishSubject, EnrolledAt = DateTime.UtcNow },
                new Enrollment { Student = studentEva, Subject = englishSubject, EnrolledAt = DateTime.UtcNow },
                new Enrollment { Student = studentEva, Subject = mathSubject, EnrolledAt = DateTime.UtcNow }
            };

            context.Enrollments.AddRange(enrollments);

            // --- 7. Physical Media Files & Media Assets ---
            var mediaFolder = Path.Combine(Directory.GetCurrentDirectory(), "SecureStorage", "media");

            if (!Directory.Exists(mediaFolder))
            {
                Directory.CreateDirectory(mediaFolder);
            }

            var sampleFileName = "seed-sample-image.png";
            var physicalPath = Path.Combine(mediaFolder, sampleFileName);

            if (!File.Exists(physicalPath))
            {
                string base64Png = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";
                byte[] imageBytes = Convert.FromBase64String(base64Png);
                await File.WriteAllBytesAsync(physicalPath, imageBytes);
            }

            var sampleMedia = new MediaAsset
            {
                LocationUrl = $"/secure/media/{sampleFileName}",
                MimeType = MediaType.Image,
                IsPrivate = false,
                Description = "A helpful diagram for the Algebra midterm.",
                Owner = teacherDenda
            };

            context.MediaAssets.Add(sampleMedia);

            // --- 8. Quizzes, Questions, and Options ---
            var mathQuiz = new Quiz
            {
                Title = "Algebra Midterm",
                Description = "Testing your basic algebra skills.",
                MaxRetries = 2,
                CreatedAt = DateTime.UtcNow,
                PublishDate = DateTime.UtcNow,
                Subject = mathSubject,
                QuizQuestions = new List<QuizQuestion>
                {
                    new QuizQuestion
                    {
                        Title = "What is the value of x if 2x = 10? (See attached diagram)",
                        Difficulty = 1,
                        TimeLimit = 30,
                        Media = sampleMedia,
                        Options = new List<QuizOption>
                        {
                            new QuizOption { Text = "4", IsCorrect = false, IsFreeText = false },
                            new QuizOption { Text = "5", IsCorrect = true, IsFreeText = false },
                            new QuizOption { Text = "10", IsCorrect = false, IsFreeText = false }
                        }
                    },
                    new QuizQuestion
                    {
                        Title = "Type the formula for the area of a circle:",
                        Difficulty = 2,
                        TimeLimit = 60,
                        Options = new List<QuizOption>
                        {
                            new QuizOption { Text = "pi*r^2", IsCorrect = true, IsFreeText = true }
                        }
                    }
                }
            };

            var englishQuiz = new Quiz
            {
                Title = "Shakespeare Pop Quiz",
                Description = "A quick check on our recent reading.",
                MaxRetries = 1,
                CreatedAt = DateTime.UtcNow,
                PublishDate = DateTime.UtcNow.AddDays(1),
                Subject = englishSubject,
                QuizQuestions = new List<QuizQuestion>
                {
                    new QuizQuestion
                    {
                        Title = "Which of the following is a tragedy by William Shakespeare?",
                        Difficulty = 1,
                        TimeLimit = 45,
                        Options = new List<QuizOption>
                        {
                            new QuizOption { Text = "A Midsummer Night's Dream", IsCorrect = false, IsFreeText = false },
                            new QuizOption { Text = "Hamlet", IsCorrect = true, IsFreeText = false },
                            new QuizOption { Text = "The Comedy of Errors", IsCorrect = false, IsFreeText = false }
                        }
                    }
                }
            };

            context.Quizzes.AddRange(mathQuiz, englishQuiz);

            // --- 9. Gamification Badges ---

            var badgesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "badges");
            if (!Directory.Exists(badgesFolder)) Directory.CreateDirectory(badgesFolder);

            string base64BadgePng = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+ip1sAAAAASUVORK5CYII=";
            byte[] badgeBytes = Convert.FromBase64String(base64BadgePng);

            if (!File.Exists(Path.Combine(badgesFolder, "perfect-score.png"))) await File.WriteAllBytesAsync(Path.Combine(badgesFolder, "perfect-score.png"), badgeBytes);
            if (!File.Exists(Path.Combine(badgesFolder, "high-scorer.png"))) await File.WriteAllBytesAsync(Path.Combine(badgesFolder, "high-scorer.png"), badgeBytes);
            if (!File.Exists(Path.Combine(badgesFolder, "participant.png"))) await File.WriteAllBytesAsync(Path.Combine(badgesFolder, "participant.png"), badgeBytes);

            var perfectScoreBadge = new Badge
            {
                Name = "Perfect Score",
                Description = "You scored 100% on a quiz! Outstanding!",
                IconUrl = "/badges/perfect-score.png",
                XpBonus = 100,
                Rules = new List<BadgeRule>
                {
                    new BadgeRule { Metric = BadgeMetric.ScorePercentage, Operator = BadgeOperator.Equals, TargetValue = 100 }
                }
            };

            var highScorerBadge = new Badge
            {
                Name = "High Scorer",
                Description = "You scored at least 80% on a quiz. Great job!",
                IconUrl = "/badges/high-scorer.png",
                XpBonus = 50,
                Rules = new List<BadgeRule>
                {
                    new BadgeRule { Metric = BadgeMetric.ScorePercentage, Operator = BadgeOperator.GreaterThanOrEqual, TargetValue = 80 }
                }
            };

            var participantBadge = new Badge
            {
                Name = "First Steps",
                Description = "You completed a quiz and earned your first points!",
                IconUrl = "/badges/participant.png",
                XpBonus = 25,
                Rules = new List<BadgeRule>
                {
                    new BadgeRule { Metric = BadgeMetric.TotalScore, Operator = BadgeOperator.GreaterThan, TargetValue = 0 }
                }
            };

            context.Badges.AddRange(perfectScoreBadge, highScorerBadge, participantBadge);

            await context.SaveChangesAsync();
        }
    }
}