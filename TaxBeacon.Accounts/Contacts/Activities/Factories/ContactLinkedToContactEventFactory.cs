using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Contacts.Activities.Factories;

public class ContactLinkedToContactEventFactory: IActivityFactory<ContactEventType>
{
    public uint Revision => 1;

    public ContactEventType EventType => ContactEventType.ContactLinkedToContact;

    public ActivityItemDto Create(string eventData)
    {
        var contactLinkedToContactEvent = JsonSerializer.Deserialize<ContactLinkedToContactEvent>(eventData);

        return new ActivityItemDto
        (
            Date: contactLinkedToContactEvent!.LinkDate,
            FullName: contactLinkedToContactEvent.ExecutorFullName,
            Message: contactLinkedToContactEvent.ToString()
        );
    }
}
