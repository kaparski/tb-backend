using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Factories;

public sealed class StateIdUpdatedEventFactory: IEntityActivityFactory
{
    public uint Revision => 1;

    public EntityEventType EventType => EntityEventType.EntityStateIdUpdated;

    public ActivityItemDto Create(string evt)
    {
        var stateIdUpdatedEvent = JsonSerializer.Deserialize<StateIdUpdatedEvent>(evt);

        return new ActivityItemDto
        (
            Date: stateIdUpdatedEvent!.UpdatedDate,
            FullName: stateIdUpdatedEvent.ExecutorFullName,
            Message: stateIdUpdatedEvent.ToString()
        );
    }
}
