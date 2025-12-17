using LMS.Models;

namespace LMS.ViewModels
{
    public class CoursePlayerViewModel
    {
        // غيرنا النوع من Course لـ Subject
        public Subject Course { get; set; }

        public Lesson CurrentLesson { get; set; }
        public int ProgressPercentage { get; set; }
    }
}