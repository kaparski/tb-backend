using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts.Activities.Models;

public sealed class ClientDeactivatedEvent: EventBase
{
    public DateTime DeactivatedDate { get; }

    public ClientDeactivatedEvent(Guid executorId,
        DateTime deactivatedDate,
        string executorFullName,
        string executorRoles)
        : base(executorId, executorFullName, executorRoles) =>
        DeactivatedDate = deactivatedDate;

    public override string ToString() => $"Client deactivated";
}
