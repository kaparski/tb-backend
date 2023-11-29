using System.Text.Json;
using TaxBeacon.Accounts.Locations.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Locations.Activities.Factories;
public class LocationUnassociatedWithEntityEventFactory: ILocationActivityFactory
{
    public uint Revision => 1;

    public LocationEventType EventType => LocationEventType.EntityUnassociated;

    public ActivityItemDto Create(string locationUnassociatedWithEntityEvent)
    {
        var locationUnassociatedEvent = JsonSerializer.Deserialize<LocationUnassociatedWithEntityEvent>(locationUnassociatedWithEntityEvent);

        return new ActivityItemDto
        (
            Date: locationUnassociatedEvent!.UnassociatedDate,
            FullName: locationUnassociatedEvent.ExecutorFullName,
            Message: locationUnassociatedEvent.ToString()
        );

    }
}
