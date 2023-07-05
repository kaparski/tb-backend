using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts.Activities.Factories;

public sealed class ClientReactivatedEventFactory: IAccountActivityFactory
{
    public uint Revision => 1;

    public AccountEventType EventType => AccountEventType.ClientReactivated;

    public ActivityItemDto Create(string accountEvent)
    {
        var entityUpdatedEvent = JsonSerializer.Deserialize<ClientReactivatedEvent>(accountEvent);

        return new ActivityItemDto
        (
            Date: entityUpdatedEvent!.ReactivatedDate,
            FullName: entityUpdatedEvent.ExecutorFullName,
            Message: entityUpdatedEvent.ToString()
        );
    }
}
