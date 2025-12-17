using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Data;
using LMS.Models;
using LMS.ViewModels;
using System.Linq;

namespace LMS.Controllers
{
    public class CourseController : Controller
    {
        private readonly AppDbContext _context;

        public CourseController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CompleteLesson(int lessonId, int courseId)
        {
            int userId = 1; // User ID ثابت للتجربة

            // 1. تسجيل إن الدرس خلص
            var existingProgress = _context.UserProgresses
                .FirstOrDefault(p => p.UserId == userId && p.LessonId == lessonId);

            if (existingProgress == null)
            {
                _context.UserProgresses.Add(new UserLessonProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    IsCompleted = true
                });
                _context.SaveChanges();
            }

            // ==========================================
            // 2. التعديل هنا: استخدمنا Subjects بدل Courses
            // ==========================================
            var subject = _context.Subjects
                .Include(s => s.Modules).ThenInclude(m => m.Lessons)
                .FirstOrDefault(s => s.Id == courseId);

            var totalLessons = subject.Modules.SelectMany(m => m.Lessons).Count();

            var completedLessons = _context.UserProgresses
                .Where(p => p.UserId == userId && p.IsCompleted && p.Lesson.Module.SubjectId == courseId)
                .Count();

            int percentage = totalLessons == 0 ? 0 : (int)((double)completedLessons / totalLessons * 100);

            // 3. تحديث الـ Enrollment
            var enrollment = _context.Enrollments
                .FirstOrDefault(e => e.StudentId == userId && e.SubjectId == courseId);

            if (enrollment != null)
            {
                enrollment.Progress = percentage;
                _context.SaveChanges();
            }

            return RedirectToAction("Watch", new { id = courseId, lessonId = lessonId });
        }

        public IActionResult Watch(int id, int? lessonId)
        {
            // ==========================================
            // التعديل الأول: Subjects بدل Courses
            // ==========================================
            var subject = _context.Subjects
                .Include(s => s.Modules)
                .ThenInclude(m => m.Lessons)
                .FirstOrDefault(s => s.Id == id); // استخدمنا المتغير id

            if (subject == null) return NotFound();

            Lesson currentLesson = null;
            if (lessonId.HasValue)
            {
                currentLesson = subject.Modules.SelectMany(m => m.Lessons).FirstOrDefault(l => l.Id == lessonId.Value);
            }
            else
            {
                currentLesson = subject.Modules.FirstOrDefault()?.Lessons.FirstOrDefault();
            }

            if (currentLesson == null) return Content("No content available.");

            int userId = 1;

            var totalLessons = subject.Modules.SelectMany(m => m.Lessons).Count();

            // ==========================================
            // التعديل الثاني (تصحيح الخطأ):
            // غيرنا courseId لـ id (لأن الباراميتر اسمه id)
            // ==========================================
            var completedLessons = _context.UserProgresses
                .Where(p => p.UserId == userId && p.IsCompleted && p.Lesson.Module.SubjectId == id)
                .Count();

            int percentage = totalLessons == 0 ? 0 : (int)((double)completedLessons / totalLessons * 100);

            var viewModel = new CoursePlayerViewModel
            {
                Course = subject, // ده هيشتغل دلوقتي عشان عدلنا الـ ViewModel
                CurrentLesson = currentLesson,
                ProgressPercentage = percentage
            };

            return View(viewModel);
        }
    }
}