using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Services.Tenants.Activities.Models;

namespace TaxBeacon.UserManagement.Services.Tenants.Activities;

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
