using System.Text.Json;
using TaxBeacon.Administration.Programs.Activities.Models;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Programs.Activities;

public sealed class ProgramReactivatedEventFactory: IProgramActivityFactory
{
    public uint Revision => 1;

    public ProgramEventType EventType => ProgramEventType.ProgramReactivatedEvent;

    public ActivityItemDto Create(string programEvent)
    {
        var programReactivatedEvent = JsonSerializer.Deserialize<ProgramReactivatedEvent>(programEvent);

        return new ActivityItemDto
        (
            Date: programReactivatedEvent!.ReactivatedDate,
            FullName: programReactivatedEvent.ExecutorFullName,
            Message: programReactivatedEvent.ToString()
        );
    }
}
