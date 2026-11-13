using System.Security.Claims;
using BilliardManagement.Common.Tenant;

namespace BilliardManagement.API.Middlewares
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
        {
            var user = context.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var role = user.FindFirstValue(ClaimTypes.Role) ?? user.FindFirstValue("role");
                var storeIdValue = user.FindFirstValue("store_id");

                if (string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    // SuperAdmin reads across all stores; writes require an explicit store
                    // via the X-Store-Id header so no data is created without a tenant.
                    var explicitStoreId = Guid.Empty;
                    var headerStoreId = context.Request.Headers["X-Store-Id"].FirstOrDefault();
                    if (Guid.TryParse(headerStoreId, out var parsedStoreId))
                        explicitStoreId = parsedStoreId;

                    tenantContext.Resolve(explicitStoreId, true);
                }
                else if (Guid.TryParse(storeIdValue, out var storeId))
                {
                    tenantContext.Resolve(storeId, false);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(
                        BilliardManagement.Common.Responses.ApiResponse<object>.Fail(
                            "Tài khoản chưa được gán cửa hàng. Vui lòng đăng nhập lại."));
                    return;
                }
            }
            else
            {
                tenantContext.Resolve(Guid.Empty, false);
            }

            await _next(context);
        }
    }
}
