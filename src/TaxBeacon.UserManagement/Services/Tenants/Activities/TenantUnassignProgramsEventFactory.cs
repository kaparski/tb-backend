using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services.Tenants.Activities.Models;

namespace TaxBeacon.UserManagement.Services.Tenants.Activities;

public class TenantUnassignProgramsEventFactory: ITenantActivityFactory
{
    public uint Revision => 1;

    public TenantEventType EventType => TenantEventType.TenantUnassignProgramEvent;

    public ActivityItemDto Create(string tenantEvent)
    {
        var tenantUnassignProgramsEvent = JsonSerializer.Deserialize<TenantUnassignProgramsEvent>(tenantEvent);

        return new ActivityItemDto
        (
            Date: tenantUnassignProgramsEvent!.UnassignDateTime,
            FullName: tenantUnassignProgramsEvent.ExecutorFullName,
            Message: tenantUnassignProgramsEvent.ToString()
        );
    }
}
