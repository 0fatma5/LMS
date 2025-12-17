using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // ضيفي المكتبة دي لو احتجتيها

namespace LMS.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "You must write event title")]
        public string Title { get; set; } // ده هنسيبه اجباري زي ما هو

        // ضيفي علامة الاستفهام (؟) عشان يبقى اختياري
        public string? Description { get; set; }

        // ده أهم واحد، لازم يبقى عليه علامة استفهام
        // عشان هو بييجي فاضي من الفورم واحنا بنملاه في الكنترولر
        [ValidateNever] // أو ممكن تستخدمي دي عشان تريحي دماغك خالص من الفاليدشن بتاعه
        public string? ImageUrl { get; set; }

        public DateTime EventDate { get; set; }

        public string? Duration { get; set; } // اختياري

        public string? SpeakerName { get; set; } // اختياري

        public string? Location { get; set; } // اختياري
    }
}