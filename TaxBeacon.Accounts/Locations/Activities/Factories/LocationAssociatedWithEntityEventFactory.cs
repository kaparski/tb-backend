using System.Text.Json;
using TaxBeacon.Accounts.Locations.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Locations.Activities.Factories;
public class LocationAssociatedWithEntityEventFactory: ILocationActivityFactory
{
    public uint Revision => 1;

    public LocationEventType EventType => LocationEventType.EntityAssociated;

    public ActivityItemDto Create(string locationAssociatedWithEntityEvent)
    {
        var locationAssociatedEvent = JsonSerializer.Deserialize<LocationAssociatedWithEntityEvent>(locationAssociatedWithEntityEvent);

        return new ActivityItemDto
        (
            Date: locationAssociatedEvent!.AssociatedDate,
            FullName: locationAssociatedEvent.ExecutorFullName,
            Message: locationAssociatedEvent.ToString()
        );

    }
}
