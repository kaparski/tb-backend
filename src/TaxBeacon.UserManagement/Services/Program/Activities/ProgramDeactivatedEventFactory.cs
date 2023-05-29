using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities.Program;

namespace TaxBeacon.UserManagement.Services.Program.Activities;

public sealed class ProgramDeactivatedEventFactory: IProgramActivityFactory
{
    public uint Revision => 1;

    public ProgramEventType EventType => ProgramEventType.ProgramDeactivatedEvent;

    public ActivityItemDto Create(string programEvent)
    {
        var programDeactivatedEvent = JsonSerializer.Deserialize<ProgramDeactivatedEvent>(programEvent);

        return new ActivityItemDto
        (
            Date: programDeactivatedEvent!.DeactivatedDate,
            FullName: programDeactivatedEvent.ExecutorFullName,
            Message: programDeactivatedEvent.ToString()
        );
    }
}
