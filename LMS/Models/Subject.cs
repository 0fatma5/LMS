using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        // جوه كلاس Subject.cs

        // 1. مفتاح الربط
        public int? CategoryId { get; set; }

        // 2. كائن الربط (Navigation Property)
        public Category? Category { get; set; }
        public string ColorClass { get; set; } = "course-software"; // كلاس اللون (للـ CSS)

        // رابط المدرس
        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual User? Teacher { get; set; }

        public ICollection<Module> Modules { get; set; } = new List<Module>();
    }
}