using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using LMS.Filters; 

namespace LMS.Controllers
{
    [AdminOnly]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // 1. صفحة تعرض كل المستخدمين
        public IActionResult Users()
        {
            var allUsers = _context.Users.ToList();
            return View(allUsers);
        }

        // 2. أمر ترقية المستخدم لمدير
        [HttpPost]\n        [ValidateAntiForgeryToken]\n        public IActionResult MakeAdmin(int id)
        {
            var user = _context.Users.Find(id);

            if (user != null)
            {
                user.Role = "Admin"; // تغيير الرتبة
                _context.SaveChanges(); // حفظ في القاعدة
            }

            return RedirectToAction("Users"); // تحديث الصفحة
        }
    }
}