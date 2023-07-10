using System.Text.Json;
using TaxBeacon.Administration.Tenants.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Tenants.Activities;

public class TenantEnteredEventFactory: ITenantActivityFactory
{
    public uint Revision => 1;

    public TenantEventType EventType => TenantEventType.TenantEnteredEvent;

    public ActivityItemDto Create(string tenantEvent)
    {
        var tenantEnteredEvent = JsonSerializer.Deserialize<TenantEnteredEvent>(tenantEvent);

        return new ActivityItemDto
        (
            Date: tenantEnteredEvent!.EnteredDate,
            FullName: tenantEnteredEvent.ExecutorFullName,
            Message: tenantEnteredEvent.ToString()
        );
    }
}
