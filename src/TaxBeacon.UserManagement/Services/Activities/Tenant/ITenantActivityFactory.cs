using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Activities.Tenant;

public interface ITenantActivityFactory
{
    public uint Revision { get; }

    public TenantEventType EventType { get; }

    public ActivityItemDto Create(string tenantEvent);
}
