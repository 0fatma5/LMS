using Microsoft.AspNetCore.Mvc;
using LMS.Data;
using LMS.Models;

namespace LMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. REGISTER (SIGN UP)
        // ==========================================

        // عرض الصفحة (GET)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // استلام البيانات (POST)
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                // هل البريد مسجل مسبقاً؟
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(user);
                }

                // حفظ المستخدم
                _context.Users.Add(user);
                _context.SaveChanges();

                // توجيه لصفحة الدخول
                return RedirectToAction("Login");
            }
            return View(user);
        }

        // ==========================================
        // 2. LOGIN (SIGN IN)
        // ==========================================

        // عرض الصفحة (GET) <-- هذا الجزء كان ناقصاً عندك ويسبب خطأ 405
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // ✅ هنا نحفظ بياناته في الذاكرة
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserRole", user.Role);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid Email or Password";
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // امسح الذاكرة
            return RedirectToAction("Index", "Home");
        }
    }
}