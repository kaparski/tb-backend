using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Factories;

public class StateIdDeletedEventFactory: IEntityActivityFactory
{
    public uint Revision => 1;

    public EntityEventType EventType => EntityEventType.EntityStateIdDeleted;

    public ActivityItemDto Create(string entityStateIdDeletedEvent)
    {
        var entityUpdatedEvent = JsonSerializer.Deserialize<StateIdDeletedEvent>(entityStateIdDeletedEvent);

        return new ActivityItemDto
        (
            Date: entityUpdatedEvent!.RemovedDate,
            FullName: entityUpdatedEvent.ExecutorFullName,
            Message: entityUpdatedEvent.ToString()
        );
    }
}
