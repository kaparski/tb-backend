using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Contacts.Activities.Factories;

public sealed class ContactAssignedToAccountEventFactory: IActivityFactory<ContactEventType>
{
    public uint Revision => 1;

    public ContactEventType EventType => ContactEventType.ContactAssignedToAccount;

    public ActivityItemDto Create(string eventData)
    {
        var contactAssignedToAccountEvent = JsonSerializer.Deserialize<ContactAssignedToAccountEvent>(eventData);

        return new ActivityItemDto
        (
            Date: contactAssignedToAccountEvent!.AssignDate,
            FullName: contactAssignedToAccountEvent.ExecutorFullName,
            Message: contactAssignedToAccountEvent.ToString()
        );
    }
}
