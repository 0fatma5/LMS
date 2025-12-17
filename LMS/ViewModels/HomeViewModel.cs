using LMS.Models; // لاستدعاء Subject و Blog

namespace LMS.ViewModels
{
    public class HomeViewModel
    {
        // القائمة الأولى: المواد
        public List<Subject> Subjects { get; set; }

        // القائمة الثانية: المقالات
    }
}