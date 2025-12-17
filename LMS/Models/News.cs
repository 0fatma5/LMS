using System.ComponentModel.DataAnnotations;

namespace LMS.Models
{
    public class News
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the title")]
        public string Title { get; set; } // عنوان الخبر

        [Required(ErrorMessage = "Please enter the content")]
        public string Description { get; set; } // تفاصيل الخبر

        public string? ImageUrl { get; set; } // صورة الخبر

        public string Author { get; set; } = "Admin"; // اسم الكاتب (ممكن نثبته أدمن أو نغيره)

        public string Category { get; set; } = "General"; // التصنيف (Technology, Business, etc.)

        public DateTime PublishedDate { get; set; } = DateTime.Now; // تاريخ النشر
    }
}