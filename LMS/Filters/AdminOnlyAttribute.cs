using LMS.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Checking the DB on each admin request prevents stale session roles from
            // retaining admin access after a role change or account deletion.
            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var isAdmin = db.Users.AsNoTracking().Any(u => u.Id == userId.Value && u.Role == "Admin");
            if (!isAdmin)
            {
                context.Result = new ForbidResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
