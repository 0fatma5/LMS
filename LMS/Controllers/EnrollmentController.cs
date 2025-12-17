using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Data;
using LMS.Models;

namespace LMS.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly AppDbContext _context;

        public EnrollmentController(AppDbContext context)
        {
            _context = context;
        }

        // صفحة الدفع (GET)
        [HttpGet]
        public async Task<IActionResult> Checkout(int courseId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // 1. هنا التعديل: بنستخدم Include عشان نجيب البيانات المرتبطة
            var course = await _context.Subjects
                .Include(s => s.Category)
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(s => s.Id == courseId);

            if (course == null) return NotFound();

            return View(course);
        }

        // تنفيذ الدفع (POST) - حل مشكلة 405
        [HttpPost]
        [ValidateAntiForgeryToken] // أمان
        public async Task<IActionResult> ConfirmPayment(int courseId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // تأكد إن الطالب مش مسجل قبل كدا
            var exists = await _context.Enrollments
                .AnyAsync(e => e.StudentId == userId && e.SubjectId == courseId);

            if (!exists)
            {
                var enrollment = new Enrollment
                {
                    StudentId = (int)userId,
                    SubjectId = courseId,
                    EnrollmentDate = DateTime.Now,
                    Progress = 0
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
            }

            // بعد ما يخلص يوديه الداشبورد
            // ممكن نبعت رسالة نجاح تظهر هناك
            TempData["SuccessMessage"] = "Enrollment Successful! Welcome to the course.";
            return RedirectToAction("StudentDashboard");
        }

        // الداشبورد
        public async Task<IActionResult> StudentDashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var myCourses = await _context.Enrollments
                .Include(e => e.Subject)
                    .ThenInclude(s => s.Teacher) // عشان لو عايز تعرض اسم المدرس
                .Include(e => e.Subject)
                    .ThenInclude(s => s.Category) // عشان لو عايز تعرض اسم القسم
                .Where(e => e.StudentId == userId)
                .ToListAsync();

            return View(myCourses);
        }
    }
}