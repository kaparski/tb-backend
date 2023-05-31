using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services.Program.Activities.Models;

namespace TaxBeacon.UserManagement.Services.Program.Activities;

public sealed class ProgramAssignmentUpdatedEventFactory: IProgramActivityFactory
{
    public uint Revision => 1;

    public ProgramEventType EventType => ProgramEventType.ProgramAssignmentUpdatedEvent;

    public ActivityItemDto Create(string programEvent)
    {
        var assignmentUpdatedEvent = JsonSerializer.Deserialize<ProgramAssignmentUpdatedEvent>(programEvent);

        return new ActivityItemDto
        (
            Date: assignmentUpdatedEvent!.UpdatedDate,
            FullName: assignmentUpdatedEvent.ExecutorFullName,
            Message: assignmentUpdatedEvent.ToString()
        );
    }
}
