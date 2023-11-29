namespace TaxBeacon.API.Controllers.EntityLocations.Requests;

public class AssociateLocationsToEntityRequest
{
    public List<Guid> LocationIds { get; init; } = new List<Guid>();
}
