using System;

namespace BilliardManagement.Common.Tenant
{
    public interface ITenantContext
    {
        Guid StoreId { get; }
        bool IsSuperAdmin { get; }
        bool IsResolved { get; }
        void Resolve(Guid storeId, bool isSuperAdmin);
    }
}
