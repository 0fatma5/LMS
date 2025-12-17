namespace LMS.Models
{
    public class UserLessonProgress
    {
        public int Id { get; set; }

        public int UserId { get; set; } // لازم تكون int مش string
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }

        public bool IsCompleted { get; set; }
    }
}