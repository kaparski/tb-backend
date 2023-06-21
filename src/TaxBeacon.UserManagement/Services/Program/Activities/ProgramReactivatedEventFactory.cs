using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Services.Program.Activities.Models;

namespace TaxBeacon.UserManagement.Services.Program.Activities;

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
