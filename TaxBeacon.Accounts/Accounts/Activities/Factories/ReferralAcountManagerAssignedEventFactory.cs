using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Accounts.Activities.Factories;

public sealed class ReferralAccountManagerAssignedEventFactory: IActivityFactory<AccountEventType>
{
    public uint Revision => 1;

    public AccountEventType EventType => AccountEventType.ReferralAccountManagerAssigned;

    public ActivityItemDto Create(string eventData)
    {
        var referralAccountManagerAssignedEvent = JsonSerializer.Deserialize<ReferralAccountManagerAssignedEvent>(eventData);

        return new ActivityItemDto
        (
            Date: referralAccountManagerAssignedEvent!.AssignedDate,
            FullName: referralAccountManagerAssignedEvent.ExecutorFullName,
            Message: referralAccountManagerAssignedEvent.ToString()
        );
    }
}
