using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace LMS.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }
        public string CoverImageUrl { get; set; }

        public List<Module> Modules { get; set; } = new List<Module>();
    }
}