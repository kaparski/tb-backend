using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Factories;

public class EntityDeactivatedEventFactory: IEntityActivityFactory
{
    public uint Revision => 1;

    public EntityEventType EventType => EntityEventType.EntityDeactivated;

    public ActivityItemDto Create(string entityDeactivatedEvent)
    {
        var entityUpdatedEvent = JsonSerializer.Deserialize<EntityDeactivatedEvent>(entityDeactivatedEvent);

        return new ActivityItemDto
        (
            Date: entityUpdatedEvent!.DeactivatedDate,
            FullName: entityUpdatedEvent.ExecutorFullName,
            Message: entityUpdatedEvent.ToString()
        );
    }
}
