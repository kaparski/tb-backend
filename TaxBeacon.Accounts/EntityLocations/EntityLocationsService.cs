using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Models;
using TaxBeacon.Accounts.Locations.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.EntityLocations;

public class EntityLocationsService: IEntityLocationService
{
    private readonly IAccountDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<EntityLocationsService> _logger;

    public EntityLocationsService(IAccountDbContext dbContext,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<EntityLocationsService> logger)
    {
        _context = dbContext;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<OneOf<Success, NotFound>> UnassociateLocationWithEntityAsync(Guid entityId,
        Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var entityLocation = await _context.EntityLocations
            .Where(x => x.LocationId == locationId && x.EntityId == entityId &&
                        x.TenantId == _currentUserService.TenantId)
            .SingleOrDefaultAsync(cancellationToken);

        if (entityLocation is null)
        {
            return new NotFound();
        }

        var userInfo = _currentUserService.UserInfo;
        var eventDateTime = _dateTimeService.UtcNow;

        var locationName = await _context.Locations
            .Where(x => x.Id == entityLocation.LocationId && x.TenantId == _currentUserService.TenantId)
            .Select(l => l.Name)
            .SingleOrDefaultAsync(cancellationToken);

        var entityName = await _context.Entities
            .Where(x => x.Id == entityLocation.EntityId && x.TenantId == _currentUserService.TenantId)
            .Select(l => l.Name)
            .SingleOrDefaultAsync(cancellationToken);

        _context.EntityActivityLogs.Add(new EntityActivityLog
        {
            EntityId = entityLocation.EntityId,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = EntityEventType.LocationUnassociated,
            Event = JsonSerializer.Serialize(new EntityUnassociatedWithLocationEvent(
                _currentUserService.UserId,
                userInfo.Roles,
                userInfo.FullName,
                eventDateTime,
                locationName))
        });

        _context.LocationActivityLogs.Add(new LocationActivityLog
        {
            LocationId = entityLocation.LocationId,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = LocationEventType.EntityUnassociated,
            Event = JsonSerializer.Serialize(new LocationUnassociatedWithEntityEvent(
                _currentUserService.UserId,
                userInfo.Roles,
                userInfo.FullName,
                eventDateTime,
                entityName))
        });

        _context.EntityLocations.Remove(entityLocation);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - State ({stateId}) was removed from entity ({entityId}) by {userId}",
            _dateTimeService.UtcNow,
            entityLocation.LocationId,
            entityLocation.EntityId,
            _currentUserService.UserId);

        return new Success();
    }

    public async Task<OneOf<Success, NotFound>> AssociateEntitiesToLocation(Guid accountId,
        Guid locationId,
        List<Guid> entityIds,
        CancellationToken cancellationToken)
    {
        var location = await _context.Locations
            .Where(x => x.TenantId == _currentUserService.TenantId && x.AccountId == accountId && x.Id == locationId)
            .Include(x => x.EntityLocations)
            .FirstOrDefaultAsync(cancellationToken);

        var entitiesExisting = await _context.Entities
            .Where(e => e.TenantId == _currentUserService.TenantId && e.AccountId == accountId &&
                        entityIds.Contains(e.Id)).Select(x => new { x.Id, x.Name }).ToListAsync(cancellationToken);

        if (location == null || entityIds.Count != entitiesExisting.Count)
        {
            return new NotFound();
        }

        var existingLinks = location.EntityLocations.Select(x => x.EntityId).ToHashSet();
        var entitiesToLink = entitiesExisting.Where(e => !existingLinks.Contains(e.Id));

        var eventDateTime = _dateTimeService.UtcNow;
        var userInfo = _currentUserService.UserInfo;

        foreach (var entity in entitiesToLink)
        {
            location.EntityLocations.Add(new EntityLocation
            {
                EntityId = entity.Id,
                Location = location,
                TenantId = _currentUserService.TenantId
            });

            _context.EntityActivityLogs.Add(new EntityActivityLog
            {
                EntityId = entity.Id,
                TenantId = _currentUserService.TenantId,
                Date = eventDateTime,
                Revision = 1,
                EventType = EntityEventType.LocationAssociated,
                Event = JsonSerializer.Serialize(new EntityAssociatedWithLocationEvent(
                    _currentUserService.UserId,
                    userInfo.Roles,
                    userInfo.FullName,
                    eventDateTime,
                    location.Name))
            });
        }

        _context.LocationActivityLogs.Add(new LocationActivityLog
        {
            LocationId = location.Id,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = LocationEventType.EntityAssociated,
            Event = JsonSerializer.Serialize(new LocationAssociatedWithEntityEvent(
                _currentUserService.UserId,
                userInfo.Roles,
                userInfo.FullName,
                eventDateTime,
                string.Join(", ", entitiesToLink.Select(x => x.Name))
            ))
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new Success();
    }

    public async Task<OneOf<Success, NotFound>> AssociateLocationsToEntity(Guid accountId,
        Guid entityId,
        List<Guid> locationIds,
        CancellationToken cancellationToken)
    {
        var entity = await _context.Entities
            .Where(x => x.TenantId == _currentUserService.TenantId && x.AccountId == accountId && x.Id == entityId)
            .Include(x => x.EntityLocations)
            .FirstOrDefaultAsync(cancellationToken);

        var locationsExisting = await _context.Locations
            .Where(e => e.TenantId == _currentUserService.TenantId && e.AccountId == accountId &&
                        locationIds.Contains(e.Id)).Select(x => new { x.Id, x.Name }).ToListAsync(cancellationToken);

        if (entity == null || locationIds.Count != locationsExisting.Count)
        {
            return new NotFound();
        }

        var existingLinks = entity.EntityLocations.Select(x => x.LocationId).ToHashSet();
        var locationsToLink = locationsExisting.Where(e => !existingLinks.Contains(e.Id));

        var eventDateTime = _dateTimeService.UtcNow;
        var userInfo = _currentUserService.UserInfo;

        foreach (var location in locationsToLink)
        {
            entity.EntityLocations.Add(new EntityLocation
            {
                Entity = entity,
                LocationId = location.Id,
                TenantId = _currentUserService.TenantId
            });

            _context.LocationActivityLogs.Add(new LocationActivityLog
            {
                LocationId = location.Id,
                TenantId = _currentUserService.TenantId,
                Date = eventDateTime,
                Revision = 1,
                EventType = LocationEventType.EntityAssociated,
                Event = JsonSerializer.Serialize(new LocationAssociatedWithEntityEvent(
                    _currentUserService.UserId,
                    userInfo.Roles,
                    userInfo.FullName,
                    eventDateTime,
                    entity.Name
                ))
            });
        }

        _context.EntityActivityLogs.Add(new EntityActivityLog
        {
            EntityId = entity.Id,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = EntityEventType.LocationAssociated,
            Event = JsonSerializer.Serialize(new EntityAssociatedWithLocationEvent(
                _currentUserService.UserId,
                userInfo.Roles,
                userInfo.FullName,
                eventDateTime,
                string.Join(", ", locationsToLink.Select(x => x.Name))))
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new Success();
    }
}
