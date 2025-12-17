using System.ComponentModel.DataAnnotations;

namespace LMS.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public int DurationMinutes { get; set; }
        public bool IsPreview { get; set; }

        public LessonType Type { get; set; }

        public string? ContentUrl { get; set; }

        public string? TextContent { get; set; }

        public int ModuleId { get; set; }
        public Module Module { get; set; }
    }
}