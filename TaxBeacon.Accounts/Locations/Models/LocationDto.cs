using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.Accounts.Locations.Models;

public record LocationDto
{
    public Guid Id { get; init; }

    public string LocationId { get; init; }

    public string Name { get; init; }

    public LocationType Type { get; init; }

    public State? State { get; init; }

    public string County { get; init; }

    public string City { get; init; }

    public Status Status { get; init; }

    public string Country { get; init; }
}
