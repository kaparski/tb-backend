using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Contacts.Activities.Factories;

public sealed class ContactReactivatedEventFactory: IActivityFactory<ContactEventType>
{
    public uint Revision => 1;

    public ContactEventType EventType => ContactEventType.ContactReactivated;

    public ActivityItemDto Create(string eventData)
    {
        var contactReactivatedEvent = JsonSerializer.Deserialize<ContactReactivatedEvent>(eventData);

        return new ActivityItemDto
        (
            Date: contactReactivatedEvent!.ReactivatedDate,
            FullName: contactReactivatedEvent.ExecutorFullName,
            Message: contactReactivatedEvent.ToString()
        );
    }
}
