using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Entities.Responses;

public record EntityResponse
{
    public Guid Id { get; init; }

    public int Fein { get; set; }

    public string Name { get; init; } = null!;

    public string? EntityId { get; init; }

    public string? City { get; init; }

    public State? State { get; init; }

    public string Type { get; init; } = null!;

    public Status Status { get; init; }
}
