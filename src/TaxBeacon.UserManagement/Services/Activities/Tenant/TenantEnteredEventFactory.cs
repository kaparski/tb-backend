using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities.Tenant;

namespace TaxBeacon.UserManagement.Services.Activities.Tenant;

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
