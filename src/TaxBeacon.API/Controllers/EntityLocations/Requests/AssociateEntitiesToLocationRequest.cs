namespace TaxBeacon.API.Controllers.EntityLocations.Requests;

public record AssociateEntitiesToLocationRequest
{
    public List<Guid> EntityIds { get; init; } = new List<Guid>();
}
