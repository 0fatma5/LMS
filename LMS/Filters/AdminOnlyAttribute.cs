using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters; 
using Microsoft.AspNetCore.Http; // تأكد من وجود هذه

namespace LMS.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 1. قراءة الرتبة من الذاكرة
            var userRole = context.HttpContext.Session.GetString("UserRole");

            // 2. إذا لم يكن المستخدم "Admin"
            if (userRole != "Admin")
            {
                // 3. اطرده فوراً لصفحة الدخول
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }
}