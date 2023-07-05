using System.Text.Json;
using TaxBeacon.Administration.Programs.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Programs.Activities;

public sealed class ProgramCreatedEventFactory: IProgramActivityFactory
{
    public uint Revision => 1;

    public ProgramEventType EventType => ProgramEventType.ProgramCreatedEvent;

    public ActivityItemDto Create(string programEvent)
    {
        var programCreatedEvent = JsonSerializer.Deserialize<ProgramCreatedEvent>(programEvent);

        return new ActivityItemDto
        (
            Date: programCreatedEvent!.CreatedDate,
            FullName: programCreatedEvent.ExecutorFullName,
            Message: programCreatedEvent.ToString()
        );
    }
}
