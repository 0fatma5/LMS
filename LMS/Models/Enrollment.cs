using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        // مين الطالب؟
        public int StudentId { get; set; }

        // اشترى أنهي مادة؟
        public int SubjectId { get; set; }
        [ForeignKey("SubjectId")]
        public Subject Subject { get; set; }

        // اشترى امتى؟
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        // حالة الكورس (مثلاً: شغال، خلص، إلخ) - عشان الداشبورد
        public int Progress { get; set; } = 0; // نسبة مئوية 0-100
    }
}