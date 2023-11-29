using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Factories;
using TaxBeacon.Accounts.Entities.Activities.Models;
using TaxBeacon.Accounts.Locations.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Locations.Activities.Factories;

public class LocationDeactivatedEventFactory: ILocationActivityFactory
{
    public uint Revision => 1;

    public LocationEventType EventType => LocationEventType.LocationDeactivated;

    public ActivityItemDto Create(string locationEvent)
    {
        var locationDeactivatedEvent = JsonSerializer.Deserialize<LocationDeactivatedEvent>(locationEvent);

        return new ActivityItemDto
        (
            Date: locationDeactivatedEvent!.DeactivatedDate,
            FullName: locationDeactivatedEvent.ExecutorFullName,
            Message: locationDeactivatedEvent.ToString()
        );
    }
}
