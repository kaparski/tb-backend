using System.Text.Json;
using TaxBeacon.Administration.Tenants.Activities.Models;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Tenants.Activities;

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
