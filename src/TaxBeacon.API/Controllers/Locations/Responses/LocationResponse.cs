using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Locations.Responses;

public record LocationResponse(
    Guid Id,
    string LocationId,
    string Name,
    LocationType Type,
    State State,
    string County,
    string City,
    Status Status);
