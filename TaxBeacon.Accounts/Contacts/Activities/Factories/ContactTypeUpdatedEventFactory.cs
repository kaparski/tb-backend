using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Contacts.Activities.Factories;

public class ContactTypeUpdatedEventFactory: IActivityFactory<ContactEventType>
{
    public uint Revision => 1;

    public ContactEventType EventType => ContactEventType.ContactTypeUpdated;

    public ActivityItemDto Create(string eventData)
    {
        var contactUpdatedEvent = JsonSerializer.Deserialize<ContactTypeUpdatedEvent>(eventData);

        return new ActivityItemDto
        (
            Date: contactUpdatedEvent!.UpdatedDate,
            FullName: contactUpdatedEvent.ExecutorFullName,
            Message: contactUpdatedEvent.ToString()
        );
    }
}
