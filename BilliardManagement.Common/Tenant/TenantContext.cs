using System;

namespace BilliardManagement.Common.Tenant
{
    public class TenantContext : ITenantContext
    {
        public Guid StoreId { get; private set; }
        public bool IsSuperAdmin { get; private set; }
        public bool IsResolved { get; private set; }

        public void Resolve(Guid storeId, bool isSuperAdmin)
        {
            StoreId = storeId;
            IsSuperAdmin = isSuperAdmin;
            IsResolved = true;
        }
    }
}
