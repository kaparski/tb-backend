using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Accounts.Activities.Factories;

public class ClientAccountManagerUnassignedEventFactory: IActivityFactory<AccountEventType>
{
    public uint Revision => 1;

    public AccountEventType EventType => AccountEventType.ClientAccountManagerUnassigned;

    public ActivityItemDto Create(string accountEvent)
    {
        var accountManagerUnassignedEvent =
            JsonSerializer.Deserialize<ClientAccountManagerUnassignedEvent>(accountEvent);

        return new ActivityItemDto
        (
            Date: accountManagerUnassignedEvent!.UnassignedDate,
            FullName: accountManagerUnassignedEvent.ExecutorFullName,
            Message: accountManagerUnassignedEvent.ToString()
        );
    }
}
