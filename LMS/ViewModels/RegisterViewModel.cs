using System.ComponentModel.DataAnnotations;

namespace LMS.ViewModels
{
    // Never bind signup requests directly to the database User entity.
    // In particular, Id and Admin role must not be client-controlled.
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must contain at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must include upper and lower case letters, a number, and a special character (@$!%*?&).")]
        public string Password { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        [RegularExpression("^(Student|Teacher)$", ErrorMessage = "Please choose Student or Teacher.")]
        public string Role { get; set; } = "Student";
    }
}
