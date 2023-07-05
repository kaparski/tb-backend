using System.Text.Json;
using TaxBeacon.Administration.Programs.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Programs.Activities;

public class ProgramOrgUnitAssignEventFactory: IProgramActivityFactory
{
    public uint Revision => 1;
    public ProgramEventType EventType => ProgramEventType.ProgramOrgUnitAssignEvent;
    public ActivityItemDto Create(string programEvent)
    {
        var tenantProgramAssignEvent = JsonSerializer.Deserialize<ProgramOrgUnitAssignEvent>(programEvent);

        return new ActivityItemDto
        (
            Date: tenantProgramAssignEvent!.AssignDateTime,
            FullName: tenantProgramAssignEvent.ExecutorFullName,
            Message: tenantProgramAssignEvent.ToString()
        );
    }
}
