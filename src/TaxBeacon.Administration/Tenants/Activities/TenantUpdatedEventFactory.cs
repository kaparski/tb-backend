using System.Text.Json;
using TaxBeacon.Administration.Tenants.Activities.Models;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Tenants.Activities;

public class TenantUpdatedEventFactory: ITenantActivityFactory
{
    public uint Revision => 1;

    public TenantEventType EventType => TenantEventType.TenantUpdatedEvent;

    public ActivityItemDto Create(string tenantEvent)
    {
        var tenantUpdatedEvent = JsonSerializer.Deserialize<TenantUpdatedEvent>(tenantEvent);

        return new ActivityItemDto
        (
            Date: tenantUpdatedEvent!.UpdatedDate,
            FullName: tenantUpdatedEvent.ExecutorFullName,
            Message: tenantUpdatedEvent.ToString()
        );
    }
}
