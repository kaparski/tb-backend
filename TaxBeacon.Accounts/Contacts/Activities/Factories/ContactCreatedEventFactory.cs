using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Contacts.Activities.Factories;

public sealed class ContactCreatedEventFactory: IActivityFactory<ContactEventType>
{
    public uint Revision => 1;

    public ContactEventType EventType => ContactEventType.ContactCreated;

    public ActivityItemDto Create(string eventData)
    {
        var contactCreatedEvent = JsonSerializer.Deserialize<ContactCreatedEvent>(eventData);

        return new ActivityItemDto
        (
            Date: contactCreatedEvent!.CreatedDate,
            FullName: contactCreatedEvent.ExecutorFullName,
            Message: contactCreatedEvent.ToString()
        );
    }
}
