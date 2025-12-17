using LMS.Data;
using LMS.Models;
using LMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // مهم جداً للـ Include
using System.Linq;

namespace LMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. الصفحة الرئيسية (Landing Page)
        // ==========================================
        public IActionResult Index()
        {
            var viewModel = new HomeViewModel
            {
                Subjects = _context.Subjects.Include(s => s.Teacher).ToList(),
            };

            return View(viewModel);
        }

        // ==========================================
        // 2. صفحة الكورسات (Catalog Page) + البحث 🔍
        // ==========================================
        // ضفت (int? categoryId) عشان لما تدوس على كاتجوري يفلتر
        public IActionResult Courses(string searchString, int? categoryId)
        {
            // 👇👇 السطر ده هو حل مشكلة الـ NullReferenceException 👇👇
            // لازم نبعت القائمة للفيو عشان السايدبار يشتغل
            ViewBag.ExistingCategories = _context.Categories.ToList();

            // 1. جلب كل المواد
            // ضفنا .Include(s => s.Category) عشان نعرض اسم الكاتجوري في الكارت
            var courses = _context.Subjects
                                  .Include(s => s.Teacher)
                                  .Include(s => s.Category)
                                  .AsQueryable();

            // 2. لو المستخدم كتب حاجة في البحث
            if (!string.IsNullOrEmpty(searchString))
            {
                courses = courses.Where(s =>
                    s.Name.Contains(searchString) ||
                    s.Description.Contains(searchString)
                );
            }

            // 3. 🆕 جديد: لو المستخدم داس على كاتجوري من القائمة الجانبية
            if (categoryId != null && categoryId != 0)
            {
                courses = courses.Where(s => s.CategoryId == categoryId);
            }

            // إرجاع النتيجة
            return View(courses.ToList());
        }
        // 1. GET: لفتح الصفحة فقط
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        // 2. POST: لاستقبال البيانات وحفظها
        [HttpPost]
        [ValidateAntiForgeryToken] // حماية
        public IActionResult Contact(Contact model)
        {
            if (ModelState.IsValid)
            {
                _context.Contacts.Add(model); // ضيف الرسالة للداتابيز
                _context.SaveChanges();       // احفظ التغييرات

                // رسالة نجاح تظهر للمستخدم
                TempData["SuccessMessage"] = "Thank you! Your message has been sent successfully.";

                return RedirectToAction("Contact"); // فضي الفورم
            }

            // لو فيه مشكلة في البيانات رجعه تاني بنفس البيانات عشان يصححها
            return View(model);
        }
        public IActionResult About()
        {
            return View();
        }
    }
}