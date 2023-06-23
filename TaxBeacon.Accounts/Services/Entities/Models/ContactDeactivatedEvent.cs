using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Services.Contacts.Models;
public sealed class ContactDeactivatedEvent: EventBase
{
    public DateTime DeactivatedDate { get; }

    public ContactDeactivatedEvent(Guid executorId, DateTime deactivatedDate, string executorFullName, string executorRoles)
        : base(executorId, executorFullName, executorRoles) => DeactivatedDate = deactivatedDate;

    public override string ToString() => $"Contact deactivated";
}
