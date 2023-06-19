using OneOf.Types;
using OneOf;
using TaxBeacon.Accounts.Services.Entities.Models;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Services.Entities;
public interface IEntityService
{
    OneOf<IQueryable<EntityDto>, NotFound> QueryEntitiesAsync(Guid accountId);

    Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid entityId, int page = 1,
    int pageSize = 10, CancellationToken cancellationToken = default);

    Task<OneOf<EntityDetailsDto, NotFound>> GetEntityDetailsAsync(Guid entityId, CancellationToken cancellationToken = default);

    Task<OneOf<EntityDetailsDto, NotFound>> UpdateTeamAsync(Guid id, UpdateEntityDto updateEntity, CancellationToken cancellationToken = default);
}
