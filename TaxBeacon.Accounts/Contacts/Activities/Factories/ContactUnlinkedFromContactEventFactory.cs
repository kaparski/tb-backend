using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Contacts.Activities.Factories;
public sealed class ContactUnlinkedFromContactEventFactory: IActivityFactory<ContactEventType>
{
    public uint Revision => 1;

    public ContactEventType EventType => ContactEventType.ContactUnlinkedFromContact;

    public ActivityItemDto Create(string eventData)
    {
        var contactUnlinkedWithContactEvent = JsonSerializer.Deserialize<ContactUnlinkedFromContactEvent>(eventData);

        return new ActivityItemDto
        (
            Date: contactUnlinkedWithContactEvent!.UnlinkDate,
            FullName: contactUnlinkedWithContactEvent.ExecutorFullName,
            Message: contactUnlinkedWithContactEvent.ToString()
        );
    }
}
