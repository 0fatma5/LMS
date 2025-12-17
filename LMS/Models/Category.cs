using System.ComponentModel.DataAnnotations;

namespace LMS.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        // العلاقة: الكاتجوري الواحد جواه مواد كتير
        // دي مهمة عشان لما تعمل Include تجيب المواد
        public List<Subject>? Subjects { get; set; }
    }
}