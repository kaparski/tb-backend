using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Models;

public class EntityReactivatedEvent: EventBase
{
    public DateTime ReactivatedDate { get; }

    public EntityReactivatedEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        DateTime reactivatedDate)
        : base(executorId, executorRoles, executorFullName) =>
        ReactivatedDate = reactivatedDate;

    public override string ToString() => "Entity reactivated";
}
