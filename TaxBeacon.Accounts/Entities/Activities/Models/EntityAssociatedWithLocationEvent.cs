using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Models;
public class EntityAssociatedWithLocationEvent: EventBase
{
    public EntityAssociatedWithLocationEvent(Guid executorId, string executorRoles, string executorFullName, DateTime associatedDate,
        string? locationsNames)
        : base(executorId, executorRoles, executorFullName)
    {
        AssociatedDate = associatedDate;
        LocationsNames = locationsNames;
    }

    public DateTime AssociatedDate { get; set; }

    public string? LocationsNames { get; set; }

    public override string ToString() => $"Entity associated with the location(s): {LocationsNames}";
}
