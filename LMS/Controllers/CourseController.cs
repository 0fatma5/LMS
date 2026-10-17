using LMS.Data;
using LMS.Models;
using LMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [ValidateAntiForgeryToken]
        public IActionResult CompleteLesson(int lessonId, int courseId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var subject = _context.Subjects
                .Include(s => s.Modules).ThenInclude(m => m.Lessons)
                .FirstOrDefault(s => s.Id == courseId);
            if (subject == null) return NotFound();

            var lessonExists = subject.Modules.SelectMany(m => m.Lessons)
                .Any(l => l.Id == lessonId);
            if (!lessonExists) return NotFound();

            bool enrolled = _context.Enrollments
                .Any(e => e.StudentId == userId.Value && e.SubjectId == courseId);
            if (!enrolled) return StatusCode(StatusCodes.Status403Forbidden);

            var existing = _context.UserProgresses
                .FirstOrDefault(p => p.UserId == userId.Value && p.LessonId == lessonId);
            if (existing == null)
            {
                _context.UserProgresses.Add(new UserLessonProgress
                {
                    UserId = userId.Value,
                    LessonId = lessonId,
                    IsCompleted = true
                });
            }
            else
            {
                existing.IsCompleted = true;
            }
            _context.SaveChanges();

            int total = subject.Modules.SelectMany(m => m.Lessons).Count();
            int completed = _context.UserProgresses
                .Count(p => p.UserId == userId.Value && p.IsCompleted
                    && p.Lesson.Module.SubjectId == courseId);
            int percent = total == 0 ? 0 : (int)((double)completed / total * 100);

            var enrollment = _context.Enrollments.FirstOrDefault(e =>
                e.StudentId == userId.Value && e.SubjectId == courseId);
            if (enrollment != null)
            {
                enrollment.Progress = percent;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Watch), new { id = courseId, lessonId });
        }

        [HttpGet]
        public IActionResult Watch(int id, int? lessonId)
        {
            var subject = _context.Subjects
                .Include(s => s.Modules).ThenInclude(m => m.Lessons)
                .FirstOrDefault(s => s.Id == id);
            if (subject == null) return NotFound();

            var lessons = subject.Modules.SelectMany(m => m.Lessons).ToList();
            var currentLesson = lessonId.HasValue
                ? lessons.FirstOrDefault(l => l.Id == lessonId.Value)
                : lessons.FirstOrDefault();
            if (currentLesson == null) return NotFound();

            int? userId = HttpContext.Session.GetInt32("UserId");
            bool hasAccess = userId.HasValue && (
                subject.TeacherId == userId.Value
                || _context.Users.Any(u => u.Id == userId.Value && u.Role == "Admin")
                || _context.Enrollments.Any(e => e.StudentId == userId.Value && e.SubjectId == id));
            // Only explicitly marked preview lessons are public.
            if (!currentLesson.IsPreview && !hasAccess)
            {
                if (!userId.HasValue) return RedirectToAction("Login", "Account");
                return RedirectToAction("Checkout", "Enrollment", new { courseId = id });
            }

            int completed = userId.HasValue
                ? _context.UserProgresses.Count(p =>
                    p.UserId == userId.Value && p.IsCompleted && p.Lesson.Module.SubjectId == id)
                : 0;
            int percent = lessons.Count == 0 ? 0 : (int)((double)completed / lessons.Count * 100);

            return View(new CoursePlayerViewModel
            {
                Course = subject,
                CurrentLesson = currentLesson,
                ProgressPercentage = percent
            });
        }
    }
}
