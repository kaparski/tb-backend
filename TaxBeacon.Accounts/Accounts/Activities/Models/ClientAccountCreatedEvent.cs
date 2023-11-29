using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts.Activities.Models;
public sealed class ClientAccountCreatedEvent: EventBase
{
    public DateTime CreatedDate { get; }

    public ClientAccountCreatedEvent(Guid executorId,
        DateTime createdDate,
        string executorFullName,
        string executorRoles)
        : base(executorId, executorFullName, executorRoles) =>
        CreatedDate = createdDate;

    public override string ToString() => $"Client account created";
}
