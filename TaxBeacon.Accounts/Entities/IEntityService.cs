using OneOf.Types;
using OneOf;

namespace TaxBeacon.Accounts.Entities;
public interface IEntityService
{
    OneOf<IQueryable<EntityDto>, NotFound> QueryEntitiesAsync(Guid accountId);
}
