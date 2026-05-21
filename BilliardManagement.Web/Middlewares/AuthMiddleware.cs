namespace BilliardManagement.Web.Middlewares
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        // Paths that do not require authentication
        private static readonly string[] PublicPaths =
        [
            "/auth/login",
            "/auth/register",
            "/auth/forgotpassword",
            "/accessdenied",
            "/error",
        ];

        // Static resource prefixes that skip auth
        private static readonly string[] StaticPrefixes =
        [
            "/lib", "/css", "/js", "/images", "/favicon"
        ];

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "/";

            // Allow static files
            if (StaticPrefixes.Any(p => path.StartsWith(p)))
            {
                await _next(context);
                return;
            }

            // Allow public pages
            if (PublicPaths.Any(p => path.StartsWith(p)))
            {
                // If already logged-in user visits login page, redirect by role
                if (path.StartsWith("/auth/login"))
                {
                    var existingToken = context.Session.GetString("JWToken");
                    var existingRole = context.Session.GetString("UserRole");
                    if (!string.IsNullOrEmpty(existingToken))
                    {
                        var dest = existingRole == "Admin" ? "/Admin/Dashboard/Index" : "/Staff/Dashboard/Index";
                        context.Response.Redirect(dest);
                        return;
                    }
                }
                await _next(context);
                return;
            }

            // Root → redirect by role
            if (path == "/" || path == "/index")
            {
                var rootToken = context.Session.GetString("JWToken");
                var rootRole = context.Session.GetString("UserRole");
                if (!string.IsNullOrEmpty(rootToken))
                {
                    var dest = rootRole == "Admin" ? "/Admin/Dashboard/Index" : "/Staff/Dashboard/Index";
                    context.Response.Redirect(dest);
                }
                else
                {
                    context.Response.Redirect("/Auth/Login");
                }
                return;
            }

            // Check authentication
            var token = context.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
            {
                context.Response.Redirect("/Auth/Login");
                return;
            }

            // Session timeout
            var lastActivity = context.Session.GetString("LastActivity");
            if (!string.IsNullOrEmpty(lastActivity) && DateTime.TryParse(lastActivity, out var lastTime))
            {
                if ((DateTime.UtcNow - lastTime).TotalMinutes > 30)
                {
                    context.Session.Clear();
                    context.Response.Redirect("/Auth/Login?reason=timeout");
                    return;
                }
            }
            context.Session.SetString("LastActivity", DateTime.UtcNow.ToString("o"));

            // Role-based path enforcement
            var role = context.Session.GetString("UserRole");

            // Admin routes — only Admin can access /Admin/*
            if (path.StartsWith("/admin") && role != "Admin")
            {
                context.Response.Redirect("/AccessDenied");
                return;
            }

            // Staff routes — only Staff can access /Staff/*
            if (path.StartsWith("/staff") && role != "Staff")
            {
                context.Response.Redirect("/AccessDenied");
                return;
            }

            await _next(context);

            if (context.Response.StatusCode == 401 || context.Response.StatusCode == 403)
            {
                context.Response.Redirect("/AccessDenied");
            }
        }
    }
}
