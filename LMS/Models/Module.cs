using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    public class Module
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } // اسم القسم (مثلاً: Introduction)

        // الربط مع الكورس (Subject)
        public int SubjectId { get; set; } // غيرناها لـ SubjectId عشان تبقي واضحة

        [ForeignKey("SubjectId")]
        public Subject Subject { get; set; }

        // العلاقة مع الدروس (الموديول الواحد فيه دروس كتير)
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}