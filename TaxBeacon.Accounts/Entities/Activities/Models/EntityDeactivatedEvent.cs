using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Models;

public class EntityDeactivatedEvent: EventBase
{
    public DateTime DeactivatedDate { get; }

    public EntityDeactivatedEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        DateTime deactivatedDate)
        : base(executorId, executorRoles, executorFullName) =>
        DeactivatedDate = deactivatedDate;

    public override string ToString() => "Entity deactivated";
}
