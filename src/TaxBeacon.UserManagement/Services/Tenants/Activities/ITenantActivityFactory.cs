using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Services.Tenants.Activities;

public interface ITenantActivityFactory
{
    public uint Revision { get; }

    public TenantEventType EventType { get; }

    public ActivityItemDto Create(string tenantEvent);
}
