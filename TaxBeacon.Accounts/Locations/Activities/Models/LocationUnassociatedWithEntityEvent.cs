using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Locations.Activities.Models;
public class LocationUnassociatedWithEntityEvent: EventBase
{
    public LocationUnassociatedWithEntityEvent(Guid executorId, string executorRoles, string executorFullName, DateTime unassociatedDate,
        string? entitiesNames)
        : base(executorId, executorRoles, executorFullName)
    {
        UnassociatedDate = unassociatedDate;
        EntitiesNames = entitiesNames;
    }

    public DateTime UnassociatedDate { get; set; }

    public string? EntitiesNames { get; set; }

    public override string ToString() => $"Location unassociated with the entity(ies): {EntitiesNames}";
}
