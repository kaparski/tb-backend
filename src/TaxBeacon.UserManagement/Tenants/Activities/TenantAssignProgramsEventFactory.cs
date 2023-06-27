using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Tenants.Activities.Models;

namespace TaxBeacon.UserManagement.Tenants.Activities;

public class TenantAssignProgramsEventFactory: ITenantActivityFactory
{
    public uint Revision => 1;
    public TenantEventType EventType => TenantEventType.TenantAssignProgramsEvent;

    public ActivityItemDto Create(string tenantEvent)
    {
        var tenantAssignProgramsEvent = JsonSerializer.Deserialize<TenantAssignProgramsEvent>(tenantEvent);

        return new ActivityItemDto
        (
            Date: tenantAssignProgramsEvent!.AssignDateTime,
            FullName: tenantAssignProgramsEvent.ExecutorFullName,
            Message: tenantAssignProgramsEvent.ToString()
        );
    }
}
