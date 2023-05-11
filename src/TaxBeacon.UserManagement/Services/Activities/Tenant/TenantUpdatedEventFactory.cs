using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities.Tenant;

namespace TaxBeacon.UserManagement.Services.Activities.Tenant;

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
