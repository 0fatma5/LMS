using Microsoft.AspNetCore.Mvc;
using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Hosting;

namespace LMS.Controllers
{
    public class EventsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EventsController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. صفحة العرض (للجميع)
        public IActionResult Index()
        {
            var events = _context.Events.OrderByDescending(e => e.EventDate).ToList();
            return View(events);
        }

        // ==========================================
        // 2. التعديل هنا: صفحة الإنشاء (Admin + Teacher)
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            // هات الرول الأول
            var role = HttpContext.Session.GetString("UserRole");

            // الشرط الجديد: لو مش آدمن "وكمان" مش تيتشر -> اطرده
            if (role != "Admin" && role != "Teacher")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // ==========================================
        // 3. التعديل هنا كمان: الحفظ (Admin + Teacher)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event model, IFormFile imageFile)
        {
            var role = HttpContext.Session.GetString("UserRole");

            // نفس الحماية هنا كمان
            if (role != "Admin" && role != "Teacher")
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    string folder = "images/events/";
                    string fileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);

                    if (!Directory.Exists(serverFolder)) Directory.CreateDirectory(serverFolder);

                    string filePath = Path.Combine(serverFolder, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    model.ImageUrl = fileName;
                }

                _context.Events.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "New Event has been added successfully! 🚀";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }


        // صفحة My Events (تعرض كل الإيفنتات بتصميم الداشبورد)
        [HttpGet]
        public IActionResult MyEvents()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Teacher")
            {
                return RedirectToAction("Index", "Home");
            }

            var events = _context.Events.OrderByDescending(e => e.EventDate).ToList();
            return View(events);
        }
        // ==========================================
        // 1. عرض صفحة تأكيد الحذف (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            // أمان: لو مش أدمن ولا مدرس -> ارجع
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Teacher")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return NotFound();

            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return NotFound();

            return View(eventItem); // افتح صفحة Delete.cshtml
        }

        // ==========================================
        // 2. تنفيذ الحذف فعلياً (POST)
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem != null)
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        // ==========================================
        // تعديل الإيفنت (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event model, IFormFile? imageFile)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Teacher") return RedirectToAction("Index", "Home");

            if (id != model.Id) return NotFound();

            ModelState.Remove("imageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEvent = await _context.Events.FindAsync(id);
                    if (existingEvent == null) return NotFound();

                    // تحديث البيانات
                    existingEvent.Title = model.Title;
                    existingEvent.Description = model.Description;
                    existingEvent.EventDate = model.EventDate;
                    existingEvent.Location = model.Location;

                    // تحديث الصورة لو وجدت
                    if (imageFile != null)
                    {
                        string folder = "images/events/";
                        string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                        string fileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(serverFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
                        existingEvent.ImageUrl = fileName;
                    }

                    _context.Update(existingEvent);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    if (!_context.Events.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}