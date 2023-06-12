using OneOf;
using OneOf.Types;
using TaxBeacon.Accounts.Locations.Models;

namespace TaxBeacon.Accounts.Locations;

public interface ILocationService
{
    OneOf<IQueryable<LocationDto>, NotFound> QueryLocations(Guid accountId);
}
