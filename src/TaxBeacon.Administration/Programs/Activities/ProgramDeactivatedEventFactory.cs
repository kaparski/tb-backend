using System.Text.Json;
using TaxBeacon.Administration.Programs.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Programs.Activities;

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
