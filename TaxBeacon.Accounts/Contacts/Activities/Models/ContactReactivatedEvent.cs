using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Contacts.Activities.Models;

public sealed class ContactReactivatedEvent: EventBase
{
    public DateTime ReactivatedDate { get; }

    public ContactReactivatedEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        DateTime reactivatedDate)
        : base(executorId, executorRoles, executorFullName) => ReactivatedDate = reactivatedDate;

    public override string ToString() => "Contact reactivated";
}
