using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Contacts.Activities.Models;

public sealed class ContactCreatedEvent: EventBase
{
    public DateTime CreatedDate { get; init; }

    public ContactCreatedEvent(Guid executorId, string executorRoles, string executorFullName, DateTime createdDate)
        : base(executorId, executorRoles, executorFullName) => CreatedDate = createdDate;

    public override string ToString() => "Contact created";
}
