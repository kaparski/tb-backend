using TaxBeacon.API.Controllers.Entities.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.API.Controllers.Locations.Responses;

public record LocationResponse
{
    public Guid Id { get; init; }

    public Guid AccountId { get; init; }

    public string LocationId { get; init; } = null!;

    public string Name { get; init; } = null!;

    public LocationType Type { get; init; }

    public State? State { get; init; }

    public string? County { get; init; }

    public string? City { get; init; }

    public Status Status { get; init; }

    public string Country { get; init; } = null!;

    public IEnumerable<EntityLocationResponse> EntityLocations { get; init; } = Enumerable.Empty<EntityLocationResponse>();

    public DateTime? StartDateTimeUtc { get; init; }

    public DateTime? EndDateTimeUtc { get; init; }

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? ReactivationDateTimeUtc { get; init; }

    public DateTime? DeactivationDateTimeUtc { get; init; }

    public string? NaicsCode { get; init; }

    public string? NaicsCodeIndustry { get; init; }
}
