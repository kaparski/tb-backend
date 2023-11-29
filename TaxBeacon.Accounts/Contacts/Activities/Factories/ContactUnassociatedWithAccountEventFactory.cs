using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Contacts.Activities.Factories;

public sealed class ContactUnassociatedWithAccountEventFactory: IActivityFactory<ContactEventType>
{
    public uint Revision => 1;

    public ContactEventType EventType => ContactEventType.ContactUnassociatedWithAccount;

    public ActivityItemDto Create(string eventData)
    {
        var contactUnasssociatedWithAccountEvent = JsonSerializer.Deserialize<ContactUnassociatedWithAccountEvent>(eventData);

        return new ActivityItemDto
        (
            Date: contactUnasssociatedWithAccountEvent!.UnassociateDate,
            FullName: contactUnasssociatedWithAccountEvent.ExecutorFullName,
            Message: contactUnasssociatedWithAccountEvent.ToString()
        );
    }
}
