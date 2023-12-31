using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Tenants.Activities;

public interface ITenantActivityFactory
{
    public uint Revision { get; }

    public TenantEventType EventType { get; }

    public ActivityItemDto Create(string tenantEvent);
}
