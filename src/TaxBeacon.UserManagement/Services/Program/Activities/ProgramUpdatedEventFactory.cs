using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities.Program;

namespace TaxBeacon.UserManagement.Services.Program.Activities;

public sealed class ProgramUpdatedEventFactory: IProgramActivityFactory
{
    public uint Revision => 1;

    public ProgramEventType EventType => ProgramEventType.ProgramUpdatedEvent;

    public ActivityItemDto Create(string programEvent)
    {
        var programUpdatedEvent = JsonSerializer.Deserialize<ProgramUpdatedEvent>(programEvent);

        return new ActivityItemDto
        (
            Date: programUpdatedEvent!.UpdatedDate,
            FullName: programUpdatedEvent.ExecutorFullName,
            Message: programUpdatedEvent.ToString()
        );
    }
}
