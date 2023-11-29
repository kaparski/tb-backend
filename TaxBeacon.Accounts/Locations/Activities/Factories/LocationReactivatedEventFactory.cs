using System.Text.Json;
using TaxBeacon.Accounts.Locations.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Locations.Activities.Factories;

public class LocationReactivatedEventFactory: ILocationActivityFactory
{
    public uint Revision => 1;

    public LocationEventType EventType => LocationEventType.LocationReactivated;

    public ActivityItemDto Create(string locationEvent)
    {
        var locationReactivatedEvent = JsonSerializer.Deserialize<LocationReactivatedEvent>(locationEvent);

        return new ActivityItemDto
        (
            Date: locationReactivatedEvent!.ReactivatedDate,
            FullName: locationReactivatedEvent.ExecutorFullName,
            Message: locationReactivatedEvent.ToString()
        );
    }
}
