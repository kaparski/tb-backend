using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Locations.Models;

public record LocationDto(
    Guid Id,
    string LocationId,
    string Name,
    LocationType Type,
    State State,
    string County,
    string City,
    Status Status
);
