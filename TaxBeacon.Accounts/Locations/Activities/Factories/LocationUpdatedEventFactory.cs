using System.Text.Json;
using TaxBeacon.Accounts.Locations.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Locations.Activities.Factories;

public class LocationUpdatedEventFactory: ILocationActivityFactory
{
    public uint Revision => 1;

    public LocationEventType EventType => LocationEventType.LocationUpdated;

    public ActivityItemDto Create(string locationEvent)
    {
        var locationUpdatedEvent = JsonSerializer.Deserialize<LocationUpdatedEvent>(locationEvent);

        return new ActivityItemDto
        (
            Date: locationUpdatedEvent!.UpdatedDate,
            FullName: locationUpdatedEvent.ExecutorFullName,
            Message: locationUpdatedEvent.ToString()
        );
    }
}
