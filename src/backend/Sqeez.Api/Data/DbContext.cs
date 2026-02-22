using Microsoft.EntityFrameworkCore;
using Sqeez.Api.Enums;
using Sqeez.Api.Models.Academics;
using Sqeez.Api.Models.Gamification;
using Sqeez.Api.Models.Media;
using Sqeez.Api.Models.QuizSystem;
using Sqeez.Api.Models.System;
using Sqeez.Api.Models.Users;

namespace Sqeez.Api.Data
{
    public class SqeezDbContext : DbContext
    {
        public SqeezDbContext(DbContextOptions<SqeezDbContext> options) : base(options) { }

        // --- DbSets (These become your database tables) ---
        public DbSet<SystemConfig> SystemConfigs { get; set; } = null!;
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Teacher> Teachers { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; } = null!;
        public DbSet<SchoolClass> SchoolClasses { get; set; } = null!;
        public DbSet<Subject> Subjects { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<Level> Levels { get; set; } = null!;
        public DbSet<Badge> Badges { get; set; } = null!;
        public DbSet<StudentBadge> StudentBadges { get; set; } = null!;
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
        public DbSet<QuizOption> QuizOptions { get; set; } = null!;
        public DbSet<QuizAttempt> QuizAttempts { get; set; } = null!;
        public DbSet<QuizQuestionResponse> QuizQuestionResponses { get; set; } = null!;
        public DbSet<MediaAsset> MediaAssets { get; set; } = null!;

        // --- Fluent API Configuration ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configure Table-Per-Hierarchy (TPH) for Users
            // This maps Student, Teacher, and Admin to a single "Users" table using the Role enum as a discriminator.
            modelBuilder.Entity<Student>()
                .ToTable("Users")
                .HasDiscriminator(s => s.Role)
                .HasValue<Student>(UserRole.Student)
                .HasValue<Teacher>(UserRole.Teacher)
                .HasValue<Admin>(UserRole.Admin);

            // 2. Configure Composite Primary Key for Many-to-Many (StudentBadge)
            modelBuilder.Entity<StudentBadge>()
                .HasKey(sb => new { sb.StudentId, sb.BadgeId });

            modelBuilder.Entity<StudentBadge>()
                .HasOne(sb => sb.Student)
                .WithMany(s => s.StudentBadges)
                .HasForeignKey(sb => sb.StudentId);

            modelBuilder.Entity<StudentBadge>()
                .HasOne(sb => sb.Badge)
                .WithMany(b => b.StudentBadges)
                .HasForeignKey(sb => sb.BadgeId);

            // 3. Prevent Cascade Delete Cycles
            // Relational databases (like Postgres) will throw an error if multiple cascade delete paths exist.
            // We restrict deletion on Enrollments so deleting a Student doesn't blindly wipe out everything.
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Subject)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuizAttempt>()
                .HasOne(qa => qa.Enrollment)
                .WithMany(e => e.QuizAttempts)
                .HasForeignKey(qa => qa.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Configure Many-to-Many between QuizOption and QuizQuestionResponse
            modelBuilder.Entity<QuizOption>()
                .HasMany(qo => qo.Responses)
                .WithMany(qqr => qqr.Options)
                .UsingEntity(j => j.ToTable("QuizOptionResponses"));
        }
    }
}