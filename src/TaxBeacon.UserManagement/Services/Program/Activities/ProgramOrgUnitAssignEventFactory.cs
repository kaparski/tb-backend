using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Services.Program.Activities.Models;

namespace TaxBeacon.UserManagement.Services.Program.Activities;

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
