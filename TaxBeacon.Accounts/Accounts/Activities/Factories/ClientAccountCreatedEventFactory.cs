using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Accounts.Activities.Factories;

public sealed class ClientAccountCreatedEventFactory: IActivityFactory<AccountEventType>
{
    public uint Revision => 1;

    public AccountEventType EventType => AccountEventType.ClientAccountCreated;

    public ActivityItemDto Create(string accountEvent)
    {
        var entityCreatedEvent = JsonSerializer.Deserialize<ClientAccountCreatedEvent>(accountEvent);

        return new ActivityItemDto
        (
            Date: entityCreatedEvent!.CreatedDate,
            FullName: entityCreatedEvent.ExecutorFullName,
            Message: entityCreatedEvent.ToString()
        );
    }
}
