using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BilliardManagement.Web.Pages
{
    /// <summary>
    /// Base page model for Admin-only pages.
    /// Redirects to Login if not authenticated or not Admin.
    /// </summary>
    public abstract class AdminPageModel : PageModel
    {
        public string CurrentUsername => HttpContext.Session.GetString("Username") ?? "Admin";
        public string CurrentFullName => HttpContext.Session.GetString("FullName") ?? "Administrator";
        public string CurrentRole => HttpContext.Session.GetString("UserRole") ?? "";

        public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
        {
            var token = HttpContext.Session.GetString("JWToken");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToPageResult("/Auth/Login");
                return;
            }

            if (role != "Admin")
            {
                context.Result = new RedirectToPageResult("/AccessDenied");
                return;
            }

            // Session timeout check (30 minutes)
            var lastActivity = HttpContext.Session.GetString("LastActivity");
            if (!string.IsNullOrEmpty(lastActivity) && DateTime.TryParse(lastActivity, out var lastTime))
            {
                if ((DateTime.UtcNow - lastTime).TotalMinutes > 30)
                {
                    HttpContext.Session.Clear();
                    context.Result = new RedirectToPageResult("/Auth/Login");
                    return;
                }
            }

            // Refresh last activity
            HttpContext.Session.SetString("LastActivity", DateTime.UtcNow.ToString("o"));

            base.OnPageHandlerExecuting(context);
        }
    }

    /// <summary>
    /// Base page model for Staff-only pages.
    /// Redirects to Login if not authenticated or not Staff.
    /// </summary>
    public abstract class StaffPageModel : PageModel
    {
        public string CurrentUsername => HttpContext.Session.GetString("Username") ?? "Staff";
        public string CurrentFullName => HttpContext.Session.GetString("FullName") ?? "Staff Member";
        public string CurrentRole => HttpContext.Session.GetString("UserRole") ?? "";

        public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
        {
            var token = HttpContext.Session.GetString("JWToken");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToPageResult("/Auth/Login");
                return;
            }

            if (role != "Staff")
            {
                context.Result = new RedirectToPageResult("/AccessDenied");
                return;
            }

            // Session timeout check (30 minutes)
            var lastActivity = HttpContext.Session.GetString("LastActivity");
            if (!string.IsNullOrEmpty(lastActivity) && DateTime.TryParse(lastActivity, out var lastTime))
            {
                if ((DateTime.UtcNow - lastTime).TotalMinutes > 30)
                {
                    HttpContext.Session.Clear();
                    context.Result = new RedirectToPageResult("/Auth/Login");
                    return;
                }
            }

            // Refresh last activity
            HttpContext.Session.SetString("LastActivity", DateTime.UtcNow.ToString("o"));

            base.OnPageHandlerExecuting(context);
        }
    }
}
