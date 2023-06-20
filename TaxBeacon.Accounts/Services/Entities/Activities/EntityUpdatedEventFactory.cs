using System.Text.Json;
using TaxBeacon.Accounts.Services.Entities.Models;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Services.Entities.Activities;
public class EntityUpdatedEventFactory: IEntityActivityFactory
{
    public uint Revision => 1;

    public EntityEventType EventType => EntityEventType.EntityUpdatedEvent;

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
