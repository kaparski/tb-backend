using System.Text.Json;
using TaxBeacon.Administration.Divisions.Activities.Models;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Divisions.Activities.Factories;

public class DivisionUpdatedEventFactory: IDivisionActivityFactory
{
    public uint Revision => 1;

    public DivisionEventType EventType => DivisionEventType.DivisionUpdatedEvent;

    public ActivityItemDto Create(string divisionEvent)
    {
        var divisionUpdatedEvent = JsonSerializer.Deserialize<DivisionUpdatedEvent>(divisionEvent);

        return new ActivityItemDto
        (
            Date: divisionUpdatedEvent!.UpdatedDate,
            FullName: divisionUpdatedEvent.ExecutorFullName,
            Message: divisionUpdatedEvent.ToString()
        );
    }
}