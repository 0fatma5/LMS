using Microsoft.AspNetCore.Mvc;
using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Http; // عشان السيشن
using System.IO; // عشان التعامل مع الملفات
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; // عشان مسار السيرفر

namespace LMS.Controllers
{
    public class NewsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NewsController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // 1. صفحة عرض الأخبار (متاحة للجميع)
        // ==========================================
        public IActionResult Index()
        {
            // بنجيب الأخبار ونرتبها من الأحدث للأقدم
            var newsList = _context.NewsArticles.OrderByDescending(n => n.PublishedDate).ToList();
            return View(newsList);
        }

        // ==========================================
        // 2. صفحة قراءة الخبر كامل (Details)
        // ==========================================
        public IActionResult Details(int id)
        {
            var newsItem = _context.NewsArticles.Find(id);
            if (newsItem == null)
            {
                return NotFound();
            }
            return View(newsItem);
        }

        // ==========================================
        // 3. صفحة الإضافة (Create) - محمية 🔒
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            // حماية: لو مش أدمن ولا مدرس -> ارجع
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Teacher")
            {
                return RedirectToAction("Index"); // أو Home
            }

            return View();
        }

        // ==========================================
        // 4. حفظ الخبر في الداتا بيز - محمية 🔒
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(News model, IFormFile imageFile)
        {
            // حماية مرة تانية
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Teacher")
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                // رفع الصورة
                if (imageFile != null)
                {
                    string folder = "images/news/";
                    string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);

                    // لو الفولدر مش موجود اعمله
                    if (!Directory.Exists(serverFolder))
                    {
                        Directory.CreateDirectory(serverFolder);
                    }

                    string fileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(serverFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    model.ImageUrl = fileName;
                }

                // حفظ البيانات
                model.PublishedDate = DateTime.Now;
                _context.NewsArticles.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
        // صفحة My News
        [HttpGet]
        public IActionResult MyNews()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Teacher")
            {
                return RedirectToAction("Index", "Home");
            }

            var news = _context.NewsArticles.OrderByDescending(n => n.PublishedDate).ToList();
            return View(news);
        }
        // ==========================================
        // 1. عرض صفحة تأكيد الحذف (GET)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Teacher")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null) return NotFound();

            var newsItem = await _context.NewsArticles.FindAsync(id);
            if (newsItem == null) return NotFound();

            return View(newsItem);
        }

        // ==========================================
        // 2. تنفيذ الحذف فعلياً (POST)
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var newsItem = await _context.NewsArticles.FindAsync(id);
            if (newsItem != null)
            {
                _context.NewsArticles.Remove(newsItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        // ==========================================
        // تعديل الخبر (POST) - الكود السليم
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, News model, IFormFile? imageFile)
        {
            // 1. حماية
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Teacher") return RedirectToAction("Index");

            if (id != model.Id) return NotFound();

            // 2. إهمال الـ Validation للصورة (لأننا ممكن منرفعش صورة جديدة)
            ModelState.Remove("imageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    // 3. هات الخبر الأصلي من الداتا بيز
                    var existingNews = await _context.NewsArticles.FindAsync(id);
                    if (existingNews == null) return NotFound();

                    // 4. تحديث البيانات الكتابية
                    existingNews.Title = model.Title;
                    existingNews.Description = model.Description;
                    existingNews.Category = model.Category;
                    existingNews.Author = model.Author;
                    // existingNews.PublishedDate = DateTime.Now; // (اختياري: لو عايز تحدث التاريخ)

                    // 5. لو فيه صورة جديدة ارفعت، غيرها.. لو مفيش، سيب القديمة
                    if (imageFile != null)
                    {
                        string folder = "images/news/";
                        string serverFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                        string fileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(serverFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        // امسح الصورة القديمة لو عايز توفر مساحة (اختياري)
                        // if (existingNews.ImageUrl != null) System.IO.File.Delete(...)

                        existingNews.ImageUrl = fileName;
                    }

                    // 6. احفظ التغييرات
                    _context.Update(existingNews);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    if (!_context.NewsArticles.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}