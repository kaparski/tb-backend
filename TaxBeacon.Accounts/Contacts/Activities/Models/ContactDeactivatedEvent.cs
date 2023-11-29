using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Contacts.Activities.Models;

public sealed class ContactDeactivatedEvent: EventBase
{
    public DateTime DeactivatedDate { get; }

    public ContactDeactivatedEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        DateTime deactivatedDate)
        : base(executorId, executorRoles, executorFullName) => DeactivatedDate = deactivatedDate;

    public override string ToString() => "Contact deactivated";
}
