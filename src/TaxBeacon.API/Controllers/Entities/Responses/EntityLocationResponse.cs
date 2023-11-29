namespace TaxBeacon.API.Controllers.Entities.Responses;

public record EntityLocationResponse
{
    public Guid EntityId { get; init; }
    public Guid LocationId { get; init; }
}
