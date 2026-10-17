using Microsoft.EntityFrameworkCore;
using LMS.Data;\nusing LMS.Models;\nusing Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// 1. إعداد قاعدة البيانات
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Password hashing (ASP.NET Core Identity hashing without changing the existing Users table)\nbuilder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();\n\n// Session requires an IDistributedCache implementation.\nbuilder.Services.AddDistributedMemoryCache();\n\n// 2. إعداد الـ Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;\n    options.Cookie.SameSite = SameSiteMode.Lax;\n    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.IsEssential = true;
});

// 3. تفعيل HttpContextAccessor (ممتاز إنك ضفتها)
builder.Services.AddHttpContextAccessor();

// 4. إعداد MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- (بداية الـ Pipeline) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// 🚨 التعديل هنا: حطينا Session في الأول عشان تجهز الذاكرة فوراً 🚨
app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();