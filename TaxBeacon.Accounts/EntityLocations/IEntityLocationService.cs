using OneOf;
using OneOf.Types;

namespace TaxBeacon.Accounts.EntityLocations;
public interface IEntityLocationService
{
    Task<OneOf<Success, NotFound>> AssociateEntitiesToLocation(Guid accountId, Guid locationId, List<Guid> entityIds, CancellationToken cancellationToken);
    Task<OneOf<Success, NotFound>> AssociateLocationsToEntity(Guid accountId, Guid entityId, List<Guid> locationIds, CancellationToken cancellationToken);
    Task<OneOf<Success, NotFound>> UnassociateLocationWithEntityAsync(Guid entityId, Guid locationId, CancellationToken cancellationToken = default);
}
