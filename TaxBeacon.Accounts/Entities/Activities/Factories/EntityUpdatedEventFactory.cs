using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Factories;

public class EntityUpdatedEventFactory: IEntityActivityFactory
{
    public uint Revision => 1;

    public EntityEventType EventType => EntityEventType.EntityUpdated;

    public ActivityItemDto Create(string teamEvent)
    {
        var entityUpdatedEvent = JsonSerializer.Deserialize<EntityUpdatedEvent>(teamEvent);

        return new ActivityItemDto
        (
            Date: entityUpdatedEvent!.UpdatedDate,
            FullName: entityUpdatedEvent.ExecutorFullName,
            Message: entityUpdatedEvent.ToString()
        );
    }
}
