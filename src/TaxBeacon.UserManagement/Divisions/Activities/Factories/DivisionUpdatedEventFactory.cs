using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Divisions.Activities.Models;

namespace TaxBeacon.UserManagement.Divisions.Activities.Factories;

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