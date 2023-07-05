using System.Text.Json;
using TaxBeacon.Administration.Tenants.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Tenants.Activities;

public class TenantExitedEventFactory: ITenantActivityFactory
{
    public uint Revision => 1;

    public TenantEventType EventType => TenantEventType.TenantExitedEvent;

    public ActivityItemDto Create(string tenantEvent)
    {
        var tenantExistedEvent = JsonSerializer.Deserialize<TenantExitedEvent>(tenantEvent);

        return new ActivityItemDto
        (
            Date: tenantExistedEvent!.ExitedDate,
            FullName: tenantExistedEvent.ExecutorFullName,
            Message: tenantExistedEvent.ToString()
        );
    }
}
