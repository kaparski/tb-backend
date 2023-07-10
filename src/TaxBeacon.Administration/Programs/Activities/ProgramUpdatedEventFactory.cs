using System.Text.Json;
using TaxBeacon.Administration.Programs.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Programs.Activities;

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
