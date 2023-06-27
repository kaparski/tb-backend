using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Programs.Activities.Models;

namespace TaxBeacon.UserManagement.Programs.Activities;

public class ProgramOrgUnitUnassignEventFactory: IProgramActivityFactory
{
    public uint Revision => 1;

    public ProgramEventType EventType => ProgramEventType.ProgramOrgUnitUnassignEvent;

    public ActivityItemDto Create(string programEvent)
    {
        var tenantProgramUnassignEvent = JsonSerializer.Deserialize<ProgramOrgUnitUnassignEvent>(programEvent);

        return new ActivityItemDto
        (
            Date: tenantProgramUnassignEvent!.UnassignDateTime,
            FullName: tenantProgramUnassignEvent.ExecutorFullName,
            Message: tenantProgramUnassignEvent.ToString()
        );
    }
}
