using LMS.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ==========================================
        // الجداول الأساسية للمشروع
        // ==========================================
        public DbSet<Subject> Subjects { get; set; } // ده هو جدول الكورسات بتاعك
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<News> NewsArticles { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<UserLessonProgress> UserProgresses { get; set; }

        // الجداول اللي ضفناها جديد للمنهج
        public DbSet<Module> Modules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }

        // (مسحنا Course لأنه مكرر مع Subject)

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // إعدادات العلاقات (مهمة جداً)
            // ==========================================

            // ربط الموديول بالكورس (Subject)
            modelBuilder.Entity<Module>()
                .HasOne(m => m.Subject)
                .WithMany(s => s.Modules)
                .HasForeignKey(m => m.SubjectId) // التأكيد على استخدام SubjectId
                .OnDelete(DeleteBehavior.Cascade); // لو حذفنا الكورس، نحذف موديولاته

            // ربط الدرس بالموديول
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Module)
                .WithMany(m => m.Lessons)
                .HasForeignKey(l => l.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==========================================
            // بيانات تجريبية (Seeding) - عدلناها لتستخدم Subject
            // ==========================================

            // 1. موديولات تجريبية (مرتبطة بكورس رقم 1 اللي عندك في الداتابيز)
            modelBuilder.Entity<Module>().HasData(
                new Module { Id = 1, SubjectId = 1, Title = "Introduction & Setup" },
                new Module { Id = 2, SubjectId = 1, Title = "Backend with .NET Core" }
            );

            // 2. دروس تجريبية
            modelBuilder.Entity<Lesson>().HasData(
                new Lesson
                {
                    Id = 1,
                    ModuleId = 1,
                    Title = "Welcome to the Course",
                    DurationMinutes = 5,
                    Type = LessonType.Video,
                    IsPreview = true,
                    ContentUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                    TextContent = ""
                },
                new Lesson
                {
                    Id = 2,
                    ModuleId = 1,
                    Title = "Course Syllabus",
                    DurationMinutes = 2,
                    Type = LessonType.File,
                    IsPreview = false,
                    ContentUrl = "#",
                    TextContent = ""
                }
            );
        }
    }
}