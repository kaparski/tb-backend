using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Factories;
using TaxBeacon.Accounts.Entities.Activities.Models;
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities.Accounts;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.Accounts.Entities;

public class EntityService: IEntityService
{
    private readonly ILogger<EntityService> _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly IAccountDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IImmutableDictionary<(EntityEventType, uint), IEntityActivityFactory> _entityActivityFactories;

    public EntityService(
        ILogger<EntityService> logger,
        IDateTimeService dateTimeService,
        IAccountDbContext context,
        ICurrentUserService currentUserService,
        IEnumerable<IEntityActivityFactory> entityActivityFactories)
    {
        _logger = logger;
        _dateTimeService = dateTimeService;
        _context = context;
        _currentUserService = currentUserService;
        _entityActivityFactories = entityActivityFactories?.ToImmutableDictionary(x => (x.EventType, x.Revision))
                                       ?? ImmutableDictionary<(EntityEventType, uint), IEntityActivityFactory>.Empty;
    }

    public OneOf<IQueryable<EntityDto>, NotFound> QueryEntitiesAsync(Guid accountId)
    {
        var tenantId = _currentUserService.TenantId;

        var accountExists = _context.Accounts.Any(acc => acc.Id == accountId && acc.TenantId == tenantId);

        if (!accountExists)
        {
            return new NotFound();
        }

        var itemDtos = _context.Entities
            .Where(l => l.AccountId == accountId)
            .ProjectToType<EntityDto>();

        return OneOf<IQueryable<EntityDto>, NotFound>.FromT0(itemDtos);
    }

    public async Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid entityId, int page = 1,
    int pageSize = 10, CancellationToken cancellationToken = default)
    {
        page = page == 0 ? 1 : page;
        pageSize = pageSize == 0 ? 10 : pageSize;

        var entity = await _context.Entities
            .Where(d => d.Id == entityId && d.TenantId == _currentUserService.TenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return new NotFound();
        }

        var entityActivityLogs = _context.EntityActivityLogs
            .Where(ua => ua.EntityId == entityId && ua.TenantId == _currentUserService.TenantId);

        var count = await entityActivityLogs.CountAsync(cancellationToken: cancellationToken);

        var pageCount = (uint)Math.Ceiling((double)count / pageSize);

        var activities = await entityActivityLogs
            .OrderByDescending(x => x.Date)
            .Skip((int)((page - 1) * pageSize))
            .Take((int)pageSize)
            .ToListAsync(cancellationToken);

        return new ActivityDto(pageCount,
            activities.Select(x => _entityActivityFactories[(x.EventType, x.Revision)].Create(x.Event)).ToList());
    }

    public async Task<OneOf<EntityDetailsDto, NotFound>> GetEntityDetailsAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        var entity = await _context
            .Entities
            .Include(e => e.StateIds)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == _currentUserService.TenantId && x.Id == entityId, cancellationToken);

        return entity is null
            ? new NotFound()
            : entity.Adapt<EntityDetailsDto>();
    }

    public async Task<OneOf<EntityDetailsDto, NotFound>> UpdateEntityAsync(Guid id, UpdateEntityDto updateEntity,
CancellationToken cancellationToken = default)
    {
        var entity = await _context
            .Entities
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == _currentUserService.TenantId, cancellationToken);

        if (entity is null)
        {
            return new NotFound();
        }

        var previousValues = JsonSerializer.Serialize(entity.Adapt<UpdateEntityDto>());
        var userInfo = _currentUserService.UserInfo;
        var eventDateTime = _dateTimeService.UtcNow;

        await _context.EntityActivityLogs.AddAsync(new EntityActivityLog
        {
            EntityId = id,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = EntityEventType.EntityUpdatedEvent,
            Event = JsonSerializer.Serialize(new EntityUpdatedEvent(
                _currentUserService.UserId,
                userInfo.Roles ?? string.Empty,
                userInfo.FullName,
                eventDateTime,
                previousValues,
                JsonSerializer.Serialize(updateEntity)))
        }, cancellationToken);

        updateEntity.Adapt(entity);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Entity ({entityId}) was updated by {@userId}",
            eventDateTime,
            id,
            _currentUserService.UserId);

        return entity.Adapt<EntityDetailsDto>();
    }
}
