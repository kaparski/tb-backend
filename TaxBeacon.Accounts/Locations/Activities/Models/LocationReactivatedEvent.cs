using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Locations.Activities.Models;

public class LocationReactivatedEvent: EventBase
{
    public DateTime ReactivatedDate { get; }

    public LocationReactivatedEvent(Guid executorId,
        DateTime reactivatedDate,
        string executorFullName,
        string executorRoles)
        : base(executorId, executorRoles, executorFullName) =>
        ReactivatedDate = reactivatedDate;

    public override string ToString() => $"Location reactivated";
}
