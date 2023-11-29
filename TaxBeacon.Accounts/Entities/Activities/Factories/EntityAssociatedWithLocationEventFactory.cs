using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Factories;
public class EntityAssociatedWithLocationEventFactory: IEntityActivityFactory
{
    public uint Revision => 1;

    public EntityEventType EventType => EntityEventType.LocationAssociated;

    public ActivityItemDto Create(string entityAssociatedWithLocationEvent)
    {
        var entityAssociatedEvent = JsonSerializer.Deserialize<EntityAssociatedWithLocationEvent>(entityAssociatedWithLocationEvent);

        return new ActivityItemDto
        (
            Date: entityAssociatedEvent!.AssociatedDate,
            FullName: entityAssociatedEvent.ExecutorFullName,
            Message: entityAssociatedEvent.ToString()
        );

    }
}
