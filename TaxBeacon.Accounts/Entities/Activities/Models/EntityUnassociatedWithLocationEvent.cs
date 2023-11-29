using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Models;
public class EntityUnassociatedWithLocationEvent: EventBase
{
    public EntityUnassociatedWithLocationEvent(Guid executorId, string executorRoles, string executorFullName, DateTime unassociatedDate,
        string? locationsNames)
        : base(executorId, executorRoles, executorFullName)
    {
        UnassociatedDate = unassociatedDate;
        LocationsNames = locationsNames;
    }

    public DateTime UnassociatedDate { get; set; }

    public string? LocationsNames { get; set; }

    public override string ToString() => $"Entity unassociated with the location(s): {LocationsNames}";
}
