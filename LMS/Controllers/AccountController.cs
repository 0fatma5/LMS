using System.Security.Cryptography;
using System.Text;
using LMS.Data;
using LMS.Models;
using LMS.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AccountController(
            AppDbContext context,
            IPasswordHasher<User> passwordHasher,
            IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel form)
        {
            if (!ModelState.IsValid) return View(form);

            string email = form.Email.Trim().ToLowerInvariant();
            if (_context.Users.Any(user => user.Email == email))
            {
                ModelState.AddModelError(nameof(form.Email), "This email is already registered.");
                return View(form);
            }

            // The client can only request Student or Teacher. Admin is never self-assigned.
            if (form.Role != "Student" && form.Role != "Teacher")
            {
                ModelState.AddModelError(nameof(form.Role), "Invalid account type.");
                return View(form);
            }

            var user = new User
            {
                FullName = form.FullName.Trim(),
                Email = email,
                PhoneNumber = form.PhoneNumber,
                Role = form.Role
            };
            // Reuse the existing database column; its value is now a password HASH.
            user.Password = _passwordHasher.HashPassword(user, form.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            string normalizedEmail = email.Trim().ToLowerInvariant();
            var user = _context.Users.FirstOrDefault(u => u.Email == normalizedEmail);
            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            PasswordVerificationResult result;
            try
            {
                result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            }
            catch (FormatException)
            {
                result = PasswordVerificationResult.Failed;
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.Password = _passwordHasher.HashPassword(user, password);
                _context.SaveChanges();
            }
            else if (result == PasswordVerificationResult.Failed)
            {
                // Old records in this learning project contain plaintext passwords.
                // Opt-in locally for a short migration window; NEVER enable publicly.
                bool allowLegacy = _configuration.GetValue<bool>("Authentication:AllowLegacyPlaintextLogin");
                var stored = Encoding.UTF8.GetBytes(user.Password);
                var candidate = Encoding.UTF8.GetBytes(password);
                bool isIdentityHash = user.Password.StartsWith("AQAAAA", StringComparison.Ordinal)
                    || user.Password.StartsWith("AA", StringComparison.Ordinal) && user.Password.Length > 70;
                bool matchedLegacy = allowLegacy && !isIdentityHash
                    && CryptographicOperations.FixedTimeEquals(stored, candidate);
                if (!matchedLegacy)
                {
                    ViewBag.Error = "Invalid email or password.";
                    return View();
                }

                // Only a successful legacy login upgrades this account to a hash.
                user.Password = _passwordHasher.HashPassword(user, password);
                _context.SaveChanges();
            }

            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
