using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Contacts.Activities.Factories;

public sealed class ContactDeactivatedEventFactory: IActivityFactory<ContactEventType>
{
    public uint Revision => 1;

    public ContactEventType EventType => ContactEventType.ContactDeactivated;

    public ActivityItemDto Create(string eventData)
    {
        var contactDeactivatedEvent = JsonSerializer.Deserialize<ContactDeactivatedEvent>(eventData);

        return new ActivityItemDto
        (
            Date: contactDeactivatedEvent!.DeactivatedDate,
            FullName: contactDeactivatedEvent.ExecutorFullName,
            Message: contactDeactivatedEvent.ToString()
        );
    }
}
