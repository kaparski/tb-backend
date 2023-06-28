using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Contacts.Activities.Models;

public sealed class ContactReactivatedEvent: EventBase
{
    public DateTime ReactivatedDate { get; }

    public ContactReactivatedEvent(Guid executorId, DateTime reactivatedDate, string executorFullName, string executorRoles)
        : base(executorId, executorFullName, executorRoles) => ReactivatedDate = reactivatedDate;

    public override string ToString() => $"Contact Reactivated";
}
