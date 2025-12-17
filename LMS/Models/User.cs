using System.ComponentModel.DataAnnotations;

namespace LMS.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        // الحل هنا: إعطاء قيمة مبدئية فارغة
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        // الحل هنا: إعطاء قيمة مبدئية فارغة
        public string Email { get; set; } = string.Empty;

        // ====================================================
        //  إعدادات كلمة المرور القوية
        // ====================================================
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        // 1. التأكد من الطول (8 أحرف على الأقل)
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        // 2. التأكد من التعقيد (حرف كبير، صغير، رقم، رمز)
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must have at least 1 Uppercase, 1 Lowercase, 1 Number, and 1 Special Character (@$!%*?&)")]
        // الحل هنا: إعطاء قيمة مبدئية فارغة
        public string Password { get; set; } = string.Empty;

        // --- باقي البيانات ---

        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}