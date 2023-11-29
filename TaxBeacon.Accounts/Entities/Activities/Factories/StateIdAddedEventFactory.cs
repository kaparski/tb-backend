using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Factories;

public class StateIdAddedEventFactory: IEntityActivityFactory
{
    public uint Revision => 1;

    public EntityEventType EventType => EntityEventType.EntityStateIdAdded;

    public ActivityItemDto Create(string evt)
    {
        var stateIdAddedEvent = JsonSerializer.Deserialize<StateIdAddedEvent>(evt);

        return new ActivityItemDto
        (
            Date: stateIdAddedEvent!.CreatedDate,
            FullName: stateIdAddedEvent.ExecutorFullName,
            Message: stateIdAddedEvent.ToString()
        );
    }
}
