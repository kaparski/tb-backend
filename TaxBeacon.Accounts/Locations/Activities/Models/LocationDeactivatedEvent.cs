using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Locations.Activities.Models;

public class LocationDeactivatedEvent: EventBase
{
    public DateTime DeactivatedDate { get; }

    public LocationDeactivatedEvent(Guid executorId,
        DateTime deactivatedDate,
        string executorFullName,
        string executorRoles)
        : base(executorId, executorRoles, executorFullName) =>
        DeactivatedDate = deactivatedDate;

    public override string ToString() => $"Location deactivated";
}
