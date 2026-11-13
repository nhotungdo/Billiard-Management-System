using System;

namespace BilliardManagement.Models.Tenant
{
    public interface ITenantEntity
    {
        Guid StoreId { get; set; }
    }
}
