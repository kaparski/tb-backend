using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Locations.Activities.Models;
public class LocationAssociatedWithEntityEvent: EventBase
{
    public LocationAssociatedWithEntityEvent(Guid executorId, string executorRoles, string executorFullName, DateTime associatedDate,
        string? entitiesNames)
        : base(executorId, executorRoles, executorFullName)
    {
        AssociatedDate = associatedDate;
        EntitiesNames = entitiesNames;
    }

    public DateTime AssociatedDate { get; set; }

    public string? EntitiesNames { get; set; }

    public override string ToString() => $"Location associated with the entity(ies): {EntitiesNames}";
}
