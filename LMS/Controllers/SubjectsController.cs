using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO; // ضروري عشان التعامل مع الملفات

namespace LMS.Controllers
{
    public class SubjectsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SubjectsController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // =============================================
        // 1. List Subjects (Index)
        // =============================================
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null) return RedirectToAction("Login", "Account");

            ViewBag.ExistingCategories = _context.Categories.ToList();

            var subjects = _context.Subjects
                .Include(s => s.Teacher)
                .Include(s => s.Category)
                .AsQueryable();

            if (userRole == "Teacher")
            {
                subjects = subjects.Where(s => s.TeacherId == userId);
            }

            return View(await subjects.ToListAsync());
        }

        // =============================================
        // 2. Create New Subject
        // =============================================

        public IActionResult Create()
        {
            ViewBag.ExistingCategories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subject subject, IFormFile? imageFile, string CategoryNameInput)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            subject.TeacherId = (int)userId;

            // --- Handle Category Logic ---
            if (!string.IsNullOrWhiteSpace(CategoryNameInput))
            {
                var categoryName = CategoryNameInput.Trim();
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());

                if (existingCategory != null)
                {
                    subject.CategoryId = existingCategory.Id;
                }
                else
                {
                    var newCategory = new Category { Name = categoryName };
                    _context.Categories.Add(newCategory);
                    await _context.SaveChangesAsync();
                    subject.CategoryId = newCategory.Id;
                }
            }

            // --- Handle Image Upload ---
            if (imageFile != null)
            {
                string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string fileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                subject.ImageUrl = fileName;
            }

            ModelState.Remove("Teacher");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                _context.Add(subject);
                await _context.SaveChangesAsync();

                // توجيه لصفحة إدارة المحتوى
                return RedirectToAction("ManageContent", new { id = subject.Id });
            }

            ViewBag.ExistingCategories = _context.Categories.ToList();
            return View(subject);
        }

        // =============================================
        // 3. Manage Course Content (Curriculum Builder)
        // =============================================

        public async Task<IActionResult> ManageContent(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            var subject = await _context.Subjects
                .Include(s => s.Modules)
                .ThenInclude(m => m.Lessons)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subject == null) return NotFound();

            if (userRole == "Teacher" && subject.TeacherId != userId)
            {
                return Unauthorized();
            }

            return View(subject);
        }

        // إضافة قسم جديد (Updated with TempData)
        [HttpPost]
        public async Task<IActionResult> AddModule(int subjectId, string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                var module = new Module { Title = title, SubjectId = subjectId };
                _context.Modules.Add(module);
                await _context.SaveChangesAsync();

                // رسالة نجاح
                TempData["Success"] = "Section added successfully!";
            }
            else
            {
                TempData["Error"] = "Section title cannot be empty!";
            }
            return RedirectToAction("ManageContent", new { id = subjectId });
        }

        [HttpPost]
        public async Task<IActionResult> AddLesson(int moduleId, int subjectId, string title, string lessonType, string? url, IFormFile? lessonFile)
        {
            // 1. التحقق من العنوان
            if (string.IsNullOrEmpty(title))
            {
                TempData["Error"] = "Lesson title is required!";
                return RedirectToAction("ManageContent", new { id = subjectId });
            }

            var lesson = new Lesson
            {
                Title = title,
                ModuleId = moduleId,
                // تحويل النص (Video/File) إلى Enum
                Type = (LessonType)Enum.Parse(typeof(LessonType), lessonType)
            };

            // 2. لو النوع فيديو -> نتأكد إن الرابط موجود
            if (lessonType == "Video")
            {
                if (string.IsNullOrEmpty(url))
                {
                    TempData["Error"] = "Video URL is required!";
                    return RedirectToAction("ManageContent", new { id = subjectId });
                }
                lesson.ContentUrl = url;
            }
            // 3. لو النوع ملف (PDF/Word) -> نرفع الملف
            else if (lessonType == "File" || lessonType == "Article") // اعتبرنا Article زي الملف مؤقتاً
            {
                if (lessonFile != null && lessonFile.Length > 0)
                {
                    // تحديد مسار الحفظ: wwwroot/uploads/materials
                    string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "materials");

                    // إنشاء المجلد لو مش موجود
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    // اسم فريد للملف عشان منع التكرار
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(lessonFile.FileName);
                    string filePath = Path.Combine(folderPath, fileName);

                    // حفظ الملف
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await lessonFile.CopyToAsync(stream);
                    }

                    // حفظ المسار في الداتابيز
                    lesson.ContentUrl = "/uploads/materials/" + fileName;
                }
                else
                {
                    TempData["Error"] = "Please upload a valid file!";
                    return RedirectToAction("ManageContent", new { id = subjectId });
                }
            }

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            // رسالة النجاح (هتظهر بـ SweetAlert)
            TempData["Success"] = "Lesson added successfully!";

            return RedirectToAction("ManageContent", new { id = subjectId });
        }

        // =============================================
        // 4. Edit Subject (Basic Info)
        // =============================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole == "Teacher" && subject.TeacherId != userId)
            {
                return Unauthorized();
            }

            ViewBag.ExistingCategories = _context.Categories.ToList();
            return View(subject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Subject model, IFormFile? imageFile)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove("Teacher");
            ModelState.Remove("Category");
            ModelState.Remove("imageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingSubject = await _context.Subjects.FindAsync(id);
                    if (existingSubject == null) return NotFound();

                    var userId = HttpContext.Session.GetInt32("UserId");
                    var userRole = HttpContext.Session.GetString("UserRole");
                    if (userRole == "Teacher" && existingSubject.TeacherId != userId) return Unauthorized();

                    existingSubject.Name = model.Name;
                    existingSubject.Description = model.Description;
                    existingSubject.Price = model.Price;

                    if (imageFile != null)
                    {
                        string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                        string fileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        existingSubject.ImageUrl = fileName;
                    }

                    _context.Update(existingSubject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Subjects.Any(e => e.Id == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ExistingCategories = _context.Categories.ToList();
            return View(model);
        }

        // =============================================
        // 5. Delete Subject
        // =============================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Teacher") return RedirectToAction("Index", "Home");

            if (id == null) return NotFound();

            var subject = await _context.Subjects
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subject == null) return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (role == "Teacher" && subject.TeacherId != userId) return Unauthorized();

            return View(subject);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject != null)
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                var role = HttpContext.Session.GetString("UserRole");

                if (role == "Teacher" && subject.TeacherId != userId) return Unauthorized();

                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        // =============================================
        // 6. Manage Lessons (Delete & Edit)
        // =============================================

        // 1. حذف درس (Delete Lesson)
        [HttpPost]
        public async Task<IActionResult> DeleteLesson(int lessonId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Module) // عشان نعرف نرجع للكورس الصح
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson != null)
            {
                // لو كان ملف مرفوع، نمسحه من السيرفر كمان لتوفير المساحة
                if (lesson.Type != LessonType.Video && !string.IsNullOrEmpty(lesson.ContentUrl))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, lesson.ContentUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Lesson deleted successfully!";

                return RedirectToAction("ManageContent", new { id = lesson.Module.SubjectId });
            }

            return NotFound();
        }

        // 2. حذف موديول بالكامل (Delete Module)
        [HttpPost]
        public async Task<IActionResult> DeleteModule(int moduleId)
        {
            var module = await _context.Modules.FindAsync(moduleId);
            if (module != null)
            {
                _context.Modules.Remove(module);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Section deleted successfully!";
                return RedirectToAction("ManageContent", new { id = module.SubjectId });
            }
            return NotFound();
        }

        // 3. صفحة تعديل الدرس (Edit Lesson GET)
        public async Task<IActionResult> EditLesson(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();
            return View(lesson);
        }

        // 4. حفظ تعديل الدرس (Edit Lesson POST)
        [HttpPost]
        public async Task<IActionResult> EditLesson(int id, string title, string? url, IFormFile? lessonFile)
        {
            var lesson = await _context.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null) return NotFound();

            lesson.Title = title;

            // لو المدرس غير الرابط أو رفع ملف جديد
            if (lesson.Type == LessonType.Video && !string.IsNullOrEmpty(url))
            {
                lesson.ContentUrl = url;
            }
            else if ((lesson.Type == LessonType.File || lesson.Type == LessonType.Article) && lessonFile != null)
            {
                // رفع الملف الجديد
                string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "materials");
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(lessonFile.FileName);
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await lessonFile.CopyToAsync(stream);
                }
                lesson.ContentUrl = "/uploads/materials/" + fileName;
            }

            _context.Update(lesson);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Lesson updated successfully!";

            return RedirectToAction("ManageContent", new { id = lesson.Module.SubjectId });
        }
    }
}