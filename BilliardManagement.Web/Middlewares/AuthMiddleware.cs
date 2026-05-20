using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BilliardManagement.Web.Middlewares
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Check if user is trying to access protected pages without a token
            if (path != null && !path.StartsWith("/auth") && !path.StartsWith("/api") && !path.StartsWith("/lib") && !path.StartsWith("/css") && !path.StartsWith("/js"))
            {
                var token = context.Session.GetString("JWToken");
                if (string.IsNullOrEmpty(token))
                {
                    context.Response.Redirect("/Auth/Login");
                    return;
                }
            }

            // Redirect authenticated users away from Login page
            if (path != null && path.StartsWith("/auth/login"))
            {
                var token = context.Session.GetString("JWToken");
                if (!string.IsNullOrEmpty(token))
                {
                    context.Response.Redirect("/Dashboard");
                    return;
                }
            }

            await _next(context);
            
            if (context.Response.StatusCode == 401 || context.Response.StatusCode == 403)
            {
                context.Response.Redirect("/Auth/Login");
            }
        }
    }
}
