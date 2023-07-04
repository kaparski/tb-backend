using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts.Activities.Models;

public sealed class ClientReactivatedEvent: EventBase
{
    public DateTime ReactivatedDate { get; }

    public ClientReactivatedEvent(Guid executorId,
        DateTime reactivatedDate,
        string executorFullName,
        string executorRoles)
        : base(executorId, executorFullName, executorRoles) =>
        ReactivatedDate = reactivatedDate;

    public override string ToString() => $"Client reactivated";
}
