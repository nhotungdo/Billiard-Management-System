using System.Security.Claims;

namespace BilliardManagement.API.Hubs
{
    public static class StoreGroups
    {
        public const string SuperAdminGroup = "superadmin";

        public static string ForStore(Guid storeId) => $"store-{storeId}";

        public static IReadOnlyList<string> ForRequest(HttpContext context)
        {
            var storeId = context.User.IsInRole("SuperAdmin")
                ? context.Request.Headers["X-Store-Id"].FirstOrDefault()
                : context.User.FindFirstValue("store_id");
            if (!Guid.TryParse(storeId, out var id) || id == Guid.Empty)
                throw new InvalidOperationException("A store is required for tenant notifications.");
            return new[] { ForStore(id), SuperAdminGroup };
        }

        public static string FromUser(ClaimsPrincipal? user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return "anonymous";

            if (user.IsInRole("SuperAdmin"))
                return SuperAdminGroup;

            var storeId = user.FindFirstValue("store_id");
            return Guid.TryParse(storeId, out var id) ? ForStore(id) : "unassigned";
        }
    }
}
