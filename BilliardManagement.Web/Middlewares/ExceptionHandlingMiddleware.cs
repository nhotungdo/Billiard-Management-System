using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace BilliardManagement.Web.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (HttpRequestException ex)
            {
                context.Response.Redirect($"/Error?message={Uri.EscapeDataString(ex.Message)}");
            }
            catch (Exception ex)
            {
                context.Response.Redirect($"/Error?message={Uri.EscapeDataString(ex.Message)}");
            }
        }
    }
}
