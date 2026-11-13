using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BilliardManagement.Web.Pages
{
    /// <summary>
    /// Base class for every page that requires an authenticated session.
    /// Handles the login redirect, the role gate and the 30-minute session timeout.
    /// </summary>
    public abstract class AuthenticatedPageModel : PageModel
    {
        public string CurrentUsername => HttpContext.Session.GetString("Username") ?? "";
        public string CurrentFullName => HttpContext.Session.GetString("FullName") ?? "";
        public string CurrentRole => HttpContext.Session.GetString("UserRole") ?? "";

        /// <summary>Roles allowed to open the page.</summary>
        protected abstract string[] AllowedRoles { get; }

        /// <summary>Extra path prefixes that require a selected store (multi-tenant context).</summary>
        protected virtual string[] StoreScopedPaths => Array.Empty<string>();

        public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
        {
            var token = HttpContext.Session.GetString("JWToken");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToPageResult("/Auth/Login");
                return;
            }

            if (role == null || !AllowedRoles.Contains(role))
            {
                context.Result = new RedirectToPageResult("/AccessDenied");
                return;
            }

            // SuperAdmin must pick a store before using the store-scoped screens.
            if (role == "SuperAdmin" && StoreScopedPaths.Length > 0 &&
                StoreScopedPaths.Any(path => HttpContext.Request.Path.StartsWithSegments(path)) &&
                !Guid.TryParse(HttpContext.Session.GetString("ViewAsStoreId"), out _))
            {
                context.Result = new RedirectToPageResult("/SuperAdmin/StoreSelect");
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
    /// Admin pages: /Admin/Dashboard, /Admin/Revenue, /Admin/Tables, /Admin/Products,
    /// /Admin/Categories, /Admin/Combos, /Admin/Staff, /Admin/Customers.
    /// </summary>
    public abstract class AdminPageModel : AuthenticatedPageModel
    {
        protected override string[] AllowedRoles => new[] { "Admin", "SuperAdmin" };

        protected override string[] StoreScopedPaths => new[]
        {
            "/Admin/Tables",
            "/Admin/Products",
            "/Admin/Categories",
            "/Admin/Combos",
            "/Admin/Staff",
            "/Admin/Customers"
        };
    }

    /// <summary>
    /// SuperAdmin-only pages: /Admin/Stores, /Admin/Users, /Admin/Reports, /SuperAdmin/*.
    /// </summary>
    public abstract class SuperAdminPageModel : AuthenticatedPageModel
    {
        protected override string[] AllowedRoles => new[] { "SuperAdmin" };
    }

    /// <summary>
    /// Pages shared by Admin and Staff (staff-facing tables / products screens),
    /// with an explicit exception that lets SuperAdmin open the personal profile.
    /// </summary>
    public abstract class AdminOrStaffPageModel : AuthenticatedPageModel
    {
        protected override string[] AllowedRoles => new[] { "Admin", "Staff", "SuperAdmin" };

        public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToPageResult("/Auth/Login");
                return;
            }

            // SuperAdmin is only allowed on the profile page.
            if (role == "SuperAdmin" && !HttpContext.Request.Path.StartsWithSegments("/Profile"))
            {
                context.Result = new RedirectToPageResult("/AccessDenied");
                return;
            }

            if (role != "Admin" && role != "Staff" && role != "SuperAdmin")
            {
                context.Result = new RedirectToPageResult("/AccessDenied");
                return;
            }

            base.OnPageHandlerExecuting(context);
        }
    }

    /// <summary>
    /// Staff-only pages: /Staff/Dashboard, /Staff/Tables, /Staff/Products, /Staff/Combos, /Staff/Revenue.
    /// </summary>
    public abstract class StaffPageModel : AuthenticatedPageModel
    {
        protected override string[] AllowedRoles => new[] { "Staff" };
    }
}
