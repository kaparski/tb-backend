using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Factories;
public class EntityUnassociatedWithLocationEventFactory: IEntityActivityFactory
{
    public uint Revision => 1;

    public EntityEventType EventType => EntityEventType.LocationUnassociated;

    public ActivityItemDto Create(string entityUnassociatedWithLocationEvent)
    {
        var entityUnassociatedEvent = JsonSerializer.Deserialize<EntityUnassociatedWithLocationEvent>(entityUnassociatedWithLocationEvent);

        return new ActivityItemDto
        (
            Date: entityUnassociatedEvent!.UnassociatedDate,
            FullName: entityUnassociatedEvent.ExecutorFullName,
            Message: entityUnassociatedEvent.ToString()
        );

    }
}
