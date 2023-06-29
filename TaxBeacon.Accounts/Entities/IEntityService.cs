using OneOf;
using OneOf.Types;
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities;
public interface IEntityService
{
    OneOf<IQueryable<EntityDto>, NotFound> QueryEntitiesAsync(Guid accountId);

    Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid entityId, int page = 1,
    int pageSize = 10, CancellationToken cancellationToken = default);

    Task<OneOf<EntityDetailsDto, NotFound>> GetEntityDetailsAsync(Guid entityId, CancellationToken cancellationToken = default);

    Task<OneOf<EntityDetailsDto, NotFound>> UpdateEntityAsync(Guid id, UpdateEntityDto updateEntity, CancellationToken cancellationToken = default);
}
