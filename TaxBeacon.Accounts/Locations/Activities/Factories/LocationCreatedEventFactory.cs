using System.Text.Json;
using TaxBeacon.Accounts.Locations.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Locations.Activities.Factories;

public sealed class LocationCreatedEventFactory: ILocationActivityFactory
{
    public uint Revision => 1;

    public LocationEventType EventType => LocationEventType.LocationCreated;

    public ActivityItemDto Create(string locationEvent)
    {
        var locationCreatedEvent = JsonSerializer.Deserialize<LocationCreatedEvent>(locationEvent);

        return new ActivityItemDto
        (
            Date: locationCreatedEvent!.CreatedDate,
            FullName: locationCreatedEvent.ExecutorFullName,
            Message: locationCreatedEvent.ToString()
        );
    }
}
