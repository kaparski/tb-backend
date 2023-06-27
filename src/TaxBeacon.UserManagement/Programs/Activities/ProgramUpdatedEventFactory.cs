using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Programs.Activities.Models;

namespace TaxBeacon.UserManagement.Programs.Activities;

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
