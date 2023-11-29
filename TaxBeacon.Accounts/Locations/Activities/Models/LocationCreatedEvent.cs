using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Locations.Activities.Models;

public sealed class LocationCreatedEvent: EventBase
{
    public DateTime CreatedDate { get; set; }

    public LocationCreatedEvent(Guid executorId, string executorRoles, string executorFullName, DateTime createdDate):
        base(executorId, executorRoles, executorFullName) => CreatedDate = createdDate;

    public override string ToString() => "Location created";
}
