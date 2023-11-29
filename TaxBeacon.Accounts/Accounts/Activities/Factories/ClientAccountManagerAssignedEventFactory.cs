using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Accounts.Activities.Factories;

public class ClientAccountManagerAssignedEventFactory: IActivityFactory<AccountEventType>
{
    public uint Revision => 1;

    public AccountEventType EventType => AccountEventType.ClientAccountManagerAssigned;

    public ActivityItemDto Create(string accountEvent)
    {
        var accountManagerAssignedEvent = JsonSerializer.Deserialize<ClientAccountManagerAssignedEvent>(accountEvent);

        return new ActivityItemDto
        (
            Date: accountManagerAssignedEvent!.AssignedDate,
            FullName: accountManagerAssignedEvent.ExecutorFullName,
            Message: accountManagerAssignedEvent.ToString()
        );
    }
}
