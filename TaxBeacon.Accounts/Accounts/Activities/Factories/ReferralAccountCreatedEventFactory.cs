using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Accounts.Activities.Factories;

public sealed class ReferralAccountCreatedEventFactory: IActivityFactory<AccountEventType>
{
    public uint Revision => 1;

    public AccountEventType EventType => AccountEventType.ReferralAccountCreated;

    public ActivityItemDto Create(string eventData)
    {
        var referralAccountCreatedEvent = JsonSerializer.Deserialize<ReferralAccountCreatedEvent>(eventData);

        return new ActivityItemDto
        (
            Date: referralAccountCreatedEvent!.CreatedDate,
            FullName: referralAccountCreatedEvent.ExecutorFullName,
            Message: referralAccountCreatedEvent.ToString()
        );
    }
}
