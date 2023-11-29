using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Factories;

public class EntityReactivatedEventFactory: IEntityActivityFactory
{
    public uint Revision => 1;

    public EntityEventType EventType => EntityEventType.EntityReactivated;

    public ActivityItemDto Create(string entityReactivatedEvent)
    {
        var entityUpdatedEvent = JsonSerializer.Deserialize<EntityReactivatedEvent>(entityReactivatedEvent);

        return new ActivityItemDto
        (
            Date: entityUpdatedEvent!.ReactivatedDate,
            FullName: entityUpdatedEvent.ExecutorFullName,
            Message: entityUpdatedEvent.ToString()
        );
    }
}
