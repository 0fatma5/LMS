using System;
using System.ComponentModel.DataAnnotations;

namespace LMS.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter your name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter phone number")]
        public string Phone { get; set; }

        public string Subject { get; set; }

        [Required(ErrorMessage = "Message cannot be empty")]
        public string Message { get; set; }

        public DateTime SentDate { get; set; } = DateTime.Now; // عشان نعرف الرسالة اتبعتت امتى
    }
}