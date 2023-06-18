using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
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

    public async Task<OneOf<Success<IQueryable<EntityDto>>, NotFound>> QueryEntitiesAsync(Guid accountId)
    {
        var accountExists = await _context.Accounts.AnyAsync(x => x.Id == accountId && x.TenantId == _currentUserService.TenantId);
        if (!accountExists)
        {
            return new NotFound();
        }

        var items = _context.Entities.Where(d => d.TenantId == _currentUserService.TenantId);

        var itemDtos = items
            .Where(x => x.AccountId == accountId)
            .Select(d => new EntityDto
            {
                Id = d.Id,
                Name = d.Name,
                City = d.City,
                State = d.State,
                Type = d.Type,
                Status = d.Status,
                EntityId = d.EntityId
            });

        return new Success<IQueryable<EntityDto>>(itemDtos);
    }
}
