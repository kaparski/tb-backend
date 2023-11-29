using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Accounts.Activities.Factories;

public class AccountProfileUpdatedEventFactory: IActivityFactory<AccountEventType>
{
    public uint Revision => 1;

    public AccountEventType EventType => AccountEventType.AccountProfileUpdated;

    public ActivityItemDto Create(string accountEvent)
    {
        var accountUpdatedEvent = JsonSerializer.Deserialize<AccountProfileUpdatedEvent>(accountEvent);

        return new ActivityItemDto
        (
            Date: accountUpdatedEvent!.UpdatedDate,
            FullName: accountUpdatedEvent.ExecutorFullName,
            Message: accountUpdatedEvent.ToString()
        );
    }
}
