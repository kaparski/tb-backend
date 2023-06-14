using OneOf.Types;
using OneOf;

namespace TaxBeacon.Accounts.Entities;
public interface IEntityService
{
    Task<OneOf<Success<IQueryable<EntityDto>>, NotFound>> QueryEntitiesAsync(Guid accountId);
}
