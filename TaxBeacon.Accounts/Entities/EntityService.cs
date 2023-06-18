using Mapster;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.Accounts.Entities;

public class EntityService: IEntityService
{
    private readonly IAccountDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    public EntityService(
        IAccountDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public OneOf<IQueryable<EntityDto>, NotFound> QueryEntitiesAsync(Guid accountId)
    {
        var tenantId = _currentUserService.TenantId;

        var accountExists = _context.Accounts.Any(acc => acc.Id == accountId && acc.TenantId == tenantId);

        if (!accountExists)
        {
            return new NotFound();
        }

        var entities = _context.Entities
            .Where(l => l.AccountId == accountId)
            .ProjectToType<EntityDto>();

        return OneOf<IQueryable<EntityDto>, NotFound>.FromT0(entities);
    }
}
