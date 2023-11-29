using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Accounts.Common.Services;
using TaxBeacon.Accounts.Entities.Activities.Factories;
using TaxBeacon.Accounts.Entities.Activities.Models;
using TaxBeacon.Accounts.Entities.Exporters;
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Entities;

public class EntityService: IEntityService
{
    private readonly ILogger<EntityService> _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly IAccountDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeFormatter _dateTimeFormatter;
    private readonly IAccountEntitiesToCsvExporter _accountEntitiesToCsvExporter;
    private readonly IAccountEntitiesToXlsxExporter _accountEntitiesToXlsxExporter;
    private readonly IImmutableDictionary<(EntityEventType, uint), IEntityActivityFactory> _entityActivityFactories;
    private readonly ICsvService _csvService;

    public EntityService(
        ILogger<EntityService> logger,
        IDateTimeService dateTimeService,
        IAccountDbContext context,
        ICurrentUserService currentUserService,
        IEnumerable<IEntityActivityFactory> entityActivityFactories,
        IDateTimeFormatter dateTimeFormatter,
        IAccountEntitiesToXlsxExporter accountEntitiesToXlsxExporter,
        IAccountEntitiesToCsvExporter accountEntitiesToCsvExporter,
        ICsvService csvService)
    {
        _logger = logger;
        _dateTimeService = dateTimeService;
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeFormatter = dateTimeFormatter;
        _accountEntitiesToCsvExporter = accountEntitiesToCsvExporter;
        _csvService = csvService;
        _accountEntitiesToXlsxExporter = accountEntitiesToXlsxExporter;
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

    public async Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid entityId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        page = page == 0 ? 1 : page;
        pageSize = pageSize == 0 ? 10 : pageSize;

        if (!await _context.Entities
                .AnyAsync(e => e.Id == entityId
                               && e.TenantId == _currentUserService.TenantId,
                    cancellationToken))
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

    public async Task<OneOf<EntityDetailsDto, NotFound>> GetEntityDetailsAsync(Guid entityId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context
            .Entities
            .Include(e => e.NaicsCode)
            .Include(e => e.StateIds)
            .Include(e => e.Phones)
            .Where(e => e.TenantId == _currentUserService.TenantId && e.Id == entityId)
            .AsNoTracking()
            .ProjectToType<EntityDetailsDto>()
            .SingleOrDefaultAsync(cancellationToken);

        return entity is null ? new NotFound() : entity;
    }

    public async Task<OneOf<EntityDetailsDto, NotFound, InvalidOperation>> UpdateEntityAsync(Guid id,
        UpdateEntityDto updateEntity,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context
            .Entities
            .Include(x => x.Phones)
            .SingleOrDefaultAsync(t => t.Id == id && t.TenantId == _currentUserService.TenantId, cancellationToken);

        if (entity is null)
        {
            return new NotFound();
        }

        var validationResult = await ValidateEntityAsync(updateEntity, id, cancellationToken);
        if (validationResult.TryPickT1(out var invalidOperation, out _))
        {
            return invalidOperation;
        }

        var previousValues = JsonSerializer.Serialize(entity.Adapt<UpdateEntityDto>());
        var userInfo = _currentUserService.UserInfo;
        var eventDateTime = _dateTimeService.UtcNow;

        _context.EntityActivityLogs.Add(new EntityActivityLog
        {
            EntityId = id,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = EntityEventType.EntityUpdated,
            Event = JsonSerializer.Serialize(new EntityUpdatedEvent(
                _currentUserService.UserId,
                userInfo.Roles ?? string.Empty,
                userInfo.FullName,
                eventDateTime,
                previousValues,
                JsonSerializer.Serialize(updateEntity)))
        });

        updateEntity.Adapt(entity);

        var currentPhoneIds = entity.Phones.Select(p => p.Id).ToArray();
        var newPhoneIds = updateEntity.Phones.Select(p => p.Id);
        var phonesToRemove = entity.Phones
            .Where(p =>
                currentPhoneIds.Except(newPhoneIds).Contains(p.Id))
            .ToList();
        var phonesToAdd = updateEntity.Phones
            .Where(p =>
                newPhoneIds.Except(currentPhoneIds).Contains(p.Id))
            .Select(p =>
            {
                var phone = p.Adapt<EntityPhone>();
                phone.EntityId = entity.Id;
                phone.TenantId = _currentUserService.TenantId;
                return phone;
            })
            .ToList();
        var phoneDtosToUpdate = updateEntity.Phones
            .Where(p => newPhoneIds.Intersect(currentPhoneIds).Contains(p.Id))
            .ToDictionary(p => p.Id);

        foreach (var phone in entity.Phones.Where(p => phoneDtosToUpdate.ContainsKey(p.Id)))
        {
            _context.EntityPhones.Update(phoneDtosToUpdate[phone.Id].Adapt(phone));
        }

        _context.EntityPhones.RemoveRange(phonesToRemove);
        await _context.EntityPhones.AddRangeAsync(phonesToAdd, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Entity ({entityId}) was updated by {userId}",
            eventDateTime,
            id,
            _currentUserService.UserId);

        return entity.Adapt<EntityDetailsDto>();
    }

    public async Task<OneOf<EntityDetailsDto, NotFound>> UpdateEntityStatusAsync(Guid entityId,
        Status status,
        CancellationToken cancellationToken = default)
    {
        var currentTenantId = _currentUserService.TenantId;
        var entity = await _context
            .Entities
            .Where(x => x.Id == entityId && x.TenantId == currentTenantId)
            .SingleOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return new NotFound();
        }

        switch (status)
        {
            case Status.Deactivated:
                entity.DeactivationDateTimeUtc = _dateTimeService.UtcNow;
                entity.ReactivationDateTimeUtc = null;
                break;
            case Status.Active:
                entity.ReactivationDateTimeUtc = _dateTimeService.UtcNow;
                entity.DeactivationDateTimeUtc = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        entity.Status = status;

        var now = _dateTimeService.UtcNow;
        var currentUserInfo = _currentUserService.UserInfo;

        var activityLog = status switch
        {
            Status.Active => new EntityActivityLog
            {
                TenantId = _currentUserService.TenantId,
                Date = now,
                Revision = 1,
                EntityId = entityId,
                Event = JsonSerializer.Serialize(
                    new EntityReactivatedEvent(_currentUserService.UserId,
                        currentUserInfo.Roles,
                        currentUserInfo.FullName,
                        now)),
                EventType = EntityEventType.EntityReactivated,
            },
            Status.Deactivated => new EntityActivityLog
            {
                TenantId = _currentUserService.TenantId,
                Date = now,
                Revision = 1,
                EntityId = entityId,
                Event = JsonSerializer.Serialize(
                    new EntityDeactivatedEvent(_currentUserService.UserId,
                        currentUserInfo.Roles,
                        currentUserInfo.FullName,
                        now)),
                EventType = EntityEventType.EntityDeactivated,
            },
            _ => throw new InvalidOperationException()
        };

        await _context.EntityActivityLogs.AddAsync(activityLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Entity ({entityId}) status was changed to {newStatus} by {userId}",
            _dateTimeService.UtcNow,
            entityId,
            status,
            _currentUserService.UserId);

        return await GetEntityDetailsAsync(entityId, cancellationToken);
    }

    public async Task<OneOf<FileStreamDto, NotFound>> ExportAccountEntitiesAsync(Guid accountId, FileType fileType,
        CancellationToken cancellationToken = default)
    {
        var account = await _context.Accounts
            .SingleOrDefaultAsync(a => a.Id == accountId && a.TenantId == _currentUserService.TenantId,
                cancellationToken);

        if (account is null)
        {
            return new NotFound();
        }

        var entities = _context.Entities
            .Where(e => e.TenantId == _currentUserService.TenantId && e.AccountId == accountId)
            .Include(e => e.EntityLocations)
                .ThenInclude(x => x.Location)
                    .ThenInclude(x => x.Phones)
            .Include(e => e.EntityLocations)
                .ThenInclude(x => x.Location)
                    .ThenInclude(x => x.NaicsCode)
            .OrderBy(e => e.Name)
            .AsSplitQuery()
            .AsNoTracking()
            .ProjectToType<EntityExportModel>()
            .AsAsyncEnumerable().WithCancellation(cancellationToken);

        var exportModel = new AccountEntitiesExportModel();
        await foreach (var e in entities)
        {
            var entityMapped = e.Adapt<EntityRow>();
            entityMapped.DateOfIncorporationView = _dateTimeFormatter.FormatDate(e.DateOfIncorporation);

            exportModel.Entities.Add(entityMapped);

            foreach (var stateId in e.StateIds)
            {
                var stateIdMapped = stateId.Adapt<StateIdRow>();
                stateIdMapped.EntityName = e.Name;
                exportModel.StateIds.Add(stateIdMapped);
            }

            foreach (var location in e.Locations)
            {
                var locationMapped = location.Adapt<LocationRow>();
                locationMapped.EntityName = e.Name;
                locationMapped.StartDate = _dateTimeFormatter.FormatDate(location.StartDateTimeUtc);
                locationMapped.EndDate = _dateTimeFormatter.FormatDate(location.EndDateTimeUtc);
                exportModel.Locations.Add(locationMapped);
            }
        };

        _logger.LogInformation("{dateTime} - Entities export was executed by {userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return fileType switch
        {
            FileType.Csv => (OneOf<FileStreamDto, NotFound>)await _accountEntitiesToCsvExporter.Export(exportModel, cancellationToken),
            FileType.Xlsx => (OneOf<FileStreamDto, NotFound>)_accountEntitiesToXlsxExporter.Export(exportModel),
            _ => throw new NotImplementedException(),
        };
    }

    public async Task<OneOf<EntityDetailsDto, NotFound, InvalidOperation>> CreateNewEntityAsync(Guid accountId,
        CreateEntityDto createEntityDto,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.Accounts
                .AnyAsync(a => a.Id == accountId && a.TenantId == _currentUserService.TenantId, cancellationToken))
        {
            return new NotFound();
        }

        var validationResult = await ValidateEntityAsync(createEntityDto, entityId: null, cancellationToken);
        if (validationResult.TryPickT1(out var invalidOperation, out _))
        {
            return invalidOperation;
        }

        var entity = createEntityDto.Adapt<Entity>();
        entity.Id = Guid.NewGuid();
        entity.AccountId = accountId;
        entity.TenantId = _currentUserService.TenantId;
        entity.Status = Status.Active;

        var eventDateTime = _dateTimeService.UtcNow;
        var currentUserInfo = _currentUserService.UserInfo;

        await _context.Entities.AddAsync(entity, cancellationToken);
        await _context.EntityPhones.AddRangeAsync(
            createEntityDto.Phones.Select(p => new EntityPhone
            {
                Id = Guid.NewGuid(),
                TenantId = _currentUserService.TenantId,
                Type = p.Type,
                Number = p.Number,
                Extension = p.Extension,
                EntityId = entity.Id,
            }), cancellationToken);

        await _context.EntityActivityLogs.AddAsync(new EntityActivityLog
        {
            EntityId = entity.Id,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = EntityEventType.EntityCreated,
            Event = JsonSerializer.Serialize(new EntityCreatedEvent(
                _currentUserService.UserId,
                currentUserInfo.Roles,
                currentUserInfo.FullName,
                eventDateTime))
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Entity ({createdEntityId}) was created by {userId}",
            eventDateTime,
            entity.Id,
            _currentUserService.UserId);

        return entity.Adapt<EntityDetailsDto>();
    }

    public IQueryable<StateIdDto> GetEntityStateIdsAsync(Guid entityId) =>
        _context
            .StateIds
            .Where(e => e.TenantId == _currentUserService.TenantId && e.EntityId == entityId)
            .ProjectToType<StateIdDto>();

    public async Task<OneOf<Success, NotFound>> RemoveStateIdFromEntityAsync(Guid entityId, Guid stateId,
        CancellationToken cancellationToken = default)
    {
        var state = await _context.StateIds
            .Where(x => x.Id == stateId && x.EntityId == entityId && x.TenantId == _currentUserService.TenantId)
            .SingleOrDefaultAsync(cancellationToken);

        if (state is null)
        {
            return new NotFound();
        }

        var userInfo = _currentUserService.UserInfo;
        var eventDateTime = _dateTimeService.UtcNow;

        _context.EntityActivityLogs.Add(new EntityActivityLog
        {
            EntityId = state.EntityId,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = EntityEventType.EntityStateIdDeleted,
            Event = JsonSerializer.Serialize(new StateIdDeletedEvent(
                _currentUserService.UserId,
                userInfo.Roles,
                userInfo.FullName,
                eventDateTime,
                state.StateIdCode))
        });

        _logger.LogInformation("{dateTime} - State ({stateId}) was removed from entity ({entityId}) by {userId}",
            _dateTimeService.UtcNow,
            state.Id,
            state.EntityId,
            _currentUserService.UserId);

        _context.StateIds.Remove(state);
        await _context.SaveChangesAsync(cancellationToken);
        return new Success();
    }

    public async Task<OneOf<List<StateIdDto>, NotFound>> AddStateIdsAsync(Guid entityId,
        List<AddStateIdDto> newStateIds,
        CancellationToken cancellationToken = default)
    {
        var isEntityExists = await _context.Entities
            .AnyAsync(e => e.Id == entityId && e.TenantId == _currentUserService.TenantId, cancellationToken);

        if (!isEntityExists)
        {
            return new NotFound();
        }

        await _context.StateIds.AddRangeAsync(newStateIds.Select(s =>
        {
            var newStateId = s.Adapt<StateId>();
            newStateId.Id = Guid.NewGuid();
            newStateId.TenantId = _currentUserService.TenantId;
            newStateId.EntityId = entityId;
            return newStateId;
        }), cancellationToken);

        var eventDateTime = _dateTimeService.UtcNow;
        var currentUserInfo = _currentUserService.UserInfo;

        await _context.EntityActivityLogs.AddAsync(new EntityActivityLog
        {
            EntityId = entityId,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = EntityEventType.EntityStateIdAdded,
            Event = JsonSerializer.Serialize(new StateIdAddedEvent(
                _currentUserService.UserId,
                currentUserInfo.Roles,
                currentUserInfo.FullName,
                eventDateTime,
                newStateIds.Select(x => x.StateIdCode).ToList())
            )
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return await GetEntityStateIdsAsync(entityId).ToListAsync(cancellationToken);
    }

    public async Task<OneOf<StateIdDto, NotFound>> UpdateStateIdAsync(Guid entityId,
        Guid stateId,
        UpdateStateIdDto updateStateIdDto,
        CancellationToken cancellationToken = default)
    {
        var dbStateId = await _context.StateIds
            .SingleOrDefaultAsync(s =>
                s.Id == stateId
                && s.EntityId == entityId
                && s.TenantId == _currentUserService.TenantId, cancellationToken);

        if (dbStateId is null)
        {
            return new NotFound();
        }

        var eventDateTime = _dateTimeService.UtcNow;
        var currentUserInfo = _currentUserService.UserInfo;
        updateStateIdDto.Adapt(dbStateId);

        _context.EntityActivityLogs.Add(new EntityActivityLog
        {
            EntityId = dbStateId.EntityId,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = EntityEventType.EntityStateIdUpdated,
            Event = JsonSerializer.Serialize(new StateIdUpdatedEvent(
                _currentUserService.UserId,
                currentUserInfo.Roles,
                currentUserInfo.FullName,
                dbStateId.StateIdCode,
                eventDateTime))
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - State ({stateId}) was removed from entity ({entityId}) by {userId}",
            eventDateTime,
            stateId,
            entityId,
            _currentUserService.UserId);

        return dbStateId.Adapt<StateIdDto>();
    }

    public async Task<OneOf<Success, InvalidOperation>> ImportAccountEntitiesAsync(Guid accountId, Stream stream,
        CancellationToken cancellationToken)
    {
        var importEntityModels = _csvService.Read<ImportEntityModel>(stream).ToList();

        var entitiesToImport = importEntityModels.Adapt<List<Entity>>();

        var eventDateTime = _dateTimeService.UtcNow;
        var currentUserInfo = _currentUserService.UserInfo;

        foreach (var entity in entitiesToImport)
        {
            entity.Id = Guid.NewGuid();
            entity.AccountId = accountId;
            entity.TenantId = _currentUserService.TenantId;
            entity.Status = Status.Active;

            if (string.IsNullOrEmpty(entity.EntityId))
            {
                var oneOfEntityId = await GenerateUniqueEntityIdAsync(cancellationToken);

                if (!oneOfEntityId.TryPickT0(out var entityId, out var error))
                    return error;

                entity.EntityId = entityId;
            }

            foreach (var phone in entity.Phones)
            {
                phone.Id = Guid.NewGuid();
                phone.EntityId = entity.Id;
                phone.TenantId = _currentUserService.TenantId;
            }

            _context.EntityActivityLogs.Add(new EntityActivityLog
            {
                EntityId = entity.Id,
                TenantId = _currentUserService.TenantId,
                Date = eventDateTime,
                Revision = 1,
                EventType = EntityEventType.EntityImported,
                Event = JsonSerializer.Serialize(new EntityImportedEvent(
                    _currentUserService.UserId,
                    currentUserInfo.Roles,
                    currentUserInfo.FullName,
                    eventDateTime))
            });
        }

        _context.Entities.AddRange(entitiesToImport);

        _context.AccountActivityLogs.Add(new AccountActivityLog
        {
            AccountId = accountId,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = AccountEventType.EntitiesImportedSuccessfully,
            AccountPartType = AccountPartActivityType.General,
            Event = JsonSerializer.Serialize(new EntitiesImportedSuccessfullyEvent(
                _currentUserService.UserId,
                currentUserInfo.Roles,
                currentUserInfo.FullName,
                entitiesToImport.Count,
                eventDateTime))
        });

        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("{dateTime} - {count} entities imported for account ({accountId}) by {userId}",
                eventDateTime,
                entitiesToImport.Count,
                accountId,
                _currentUserService.UserId);

            return new Success();
        }
        catch (Exception ex)
        {
            _context.ChangeTracker.Clear();
            _context.AccountActivityLogs.Add(new AccountActivityLog
            {
                AccountId = accountId,
                TenantId = _currentUserService.TenantId,
                Date = eventDateTime,
                Revision = 1,
                EventType = AccountEventType.EntitiesImportFailed,
                AccountPartType = AccountPartActivityType.General,
                Event = JsonSerializer.Serialize(new EntitiesImportFailedEvent(
                    _currentUserService.UserId,
                    currentUserInfo.Roles,
                    currentUserInfo.FullName,
                    entitiesToImport.Count,
                    eventDateTime))
            });

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogError(ex, "Entities import exception");

            return new InvalidOperation(ex.Message);
        }
    }

    private async Task<OneOf<Success, InvalidOperation>> ValidateEntityAsync(IValidateEntityModel entityDto,
        Guid? entityId,
        CancellationToken cancellationToken)
    {
        var existedEntities = await _context.Entities.Where(e =>
                e.TenantId == _currentUserService.TenantId
                && e.Id != entityId
                && (e.Name == entityDto.Name
                    || e.EntityId == entityDto.EntityId
                    || entityDto.Fein != null && e.Fein == entityDto.Fein
                    || entityDto.Ein != null && e.Ein == entityDto.Ein))
            .ToListAsync(cancellationToken);

        if (existedEntities.Any())
        {
            if (existedEntities.Any(e => e.Name == entityDto.Name))
            {
                return new InvalidOperation($"Entity with the same name {entityDto.Name} already exists",
                    nameof(entityDto.Name));
            }

            if (existedEntities.Any(e => e.EntityId == entityDto.EntityId))
            {
                return new InvalidOperation($"Entity with the same entity ID {entityDto.EntityId} already exists",
                    nameof(entityDto.EntityId));
            }

            if (existedEntities.Any(e => !string.IsNullOrEmpty(e.Fein) && e.Fein == entityDto.Fein))
            {
                return new InvalidOperation($"Entity with the same FEIN '{entityDto.Fein}' already exists",
                    nameof(entityDto.Fein));
            }

            if (existedEntities.Any(e => !string.IsNullOrEmpty(e.Ein) && e.Ein == entityDto.Ein))
            {
                return new InvalidOperation($"Entity with the same EIN '{entityDto.Ein}' already exists",
                    nameof(entityDto.Ein));
            }
        }

        return new Success();
    }

    public async Task<OneOf<string, InvalidOperation>> GenerateUniqueEntityIdAsync(CancellationToken cancellationToken)
    {
        var r = new Random();
        var code = "E" + r.Next(1_000_000, 9_999_999).ToString();
        var safeExit = 0;
        var tenantId = _currentUserService.TenantId;
        while (safeExit < 10 && await _context.Entities.AnyAsync(x => x.EntityId == code && x.TenantId == tenantId, cancellationToken))
        {
            code = "E" + r.Next(1_000_000, 9_999_999).ToString();
            safeExit++;
        }
        if (safeExit >= 10)
        {
            _logger.LogError("Failed to generate entity Id. Attempts count exceeded the limit.");
            return new InvalidOperation("Failed to generate the code");
        }
        return code;
    }
}
