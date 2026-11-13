using System.Security.Claims;
using BilliardManagement.Common.Tenant;
using BilliardManagement.Common.Exceptions;
using BilliardManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BilliardManagement.API.Middlewares
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, BilliardManagementDbContext db)
        {
            var user = context.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var role = user.FindFirstValue(ClaimTypes.Role) ?? user.FindFirstValue("role");
                var storeIdValue = user.FindFirstValue("store_id");
                var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdValue, out var userId))
                    throw new CustomException("Token không hợp lệ. Vui lòng đăng nhập lại.", 401);
                var account = await db.Users.IgnoreQueryFilters().AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.IsActive, u.Role, u.StoreId, StoreActive = u.Store != null && u.Store.IsActive })
                    .FirstOrDefaultAsync();
                if (account == null || !account.IsActive || account.Role.ToString() != role ||
                    (account.StoreId?.ToString() ?? "") != (storeIdValue ?? ""))
                    throw new CustomException("Quyền hoặc cửa hàng của tài khoản đã thay đổi. Vui lòng đăng nhập lại.", 401);
                if (role != "SuperAdmin" && !account.StoreActive)
                    throw new CustomException("Cửa hàng đã ngừng hoạt động.", 403);

                if (string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    // SuperAdmin reads across all stores; writes require an explicit store
                    // via the X-Store-Id header so no data is created without a tenant.
                    var explicitStoreId = Guid.Empty;
                    var headerStoreId = context.Request.Headers["X-Store-Id"].FirstOrDefault();
                    if (headerStoreId != null)
                    {
                        if (!Guid.TryParse(headerStoreId, out explicitStoreId) || explicitStoreId == Guid.Empty)
                            throw new CustomException("Header X-Store-Id không hợp lệ.", 400);
                        if (!await db.Stores.AnyAsync(s => s.Id == explicitStoreId && s.IsActive))
                            throw new CustomException("Cửa hàng không tồn tại hoặc đã ngừng hoạt động.", 400);
                    }

                    var tenantRoutes = new[] { "/api/tables", "/api/products", "/api/categories", "/api/combos",
                        "/api/customers", "/api/orders", "/api/sessions", "/api/table-sessions", "/api/bills", "/api/shifts" };
                    var isWrite = !HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method)
                        && !HttpMethods.IsOptions(context.Request.Method);
                    if (isWrite && explicitStoreId == Guid.Empty &&
                        tenantRoutes.Any(p => context.Request.Path.StartsWithSegments(p)))
                        throw new CustomException("SuperAdmin phải chọn cửa hàng bằng header X-Store-Id.", 400);

                    tenantContext.Resolve(explicitStoreId, true);
                }
                else if (Guid.TryParse(storeIdValue, out var storeId) && storeId != Guid.Empty)
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
