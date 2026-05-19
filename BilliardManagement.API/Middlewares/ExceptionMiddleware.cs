using System.Net;
using System.Text.Json;
using BilliardManagement.Common.Exceptions;
using BilliardManagement.Common.Responses;

namespace BilliardManagement.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "Internal Server Error from the custom middleware.";

            if (exception is CustomException customException)
            {
                statusCode = customException.StatusCode;
                message = customException.Message;
            }

            context.Response.StatusCode = statusCode;

            var result = JsonSerializer.Serialize(ApiResponse<object>.Fail(message));
            return context.Response.WriteAsync(result);
        }
    }
}
