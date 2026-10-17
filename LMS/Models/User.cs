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

        // Stored value is a salted password hash. Validation of the *input*
        // belongs to RegisterViewModel, not the persistence model.
        [Required]
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