using Gridify;
using Gridify.EntityFramework;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npoi.Mapper;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using System.Text.Json;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Roles;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services.Tenants.Activities;
using TaxBeacon.UserManagement.Services.Tenants.Activities.Models;
using TaxBeacon.UserManagement.Services.Tenants.Models;

namespace TaxBeacon.UserManagement.Services.Tenants;

public class TenantService: ITenantService
{
    private readonly ILogger<TenantService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
    private readonly IDateTimeFormatter _dateTimeFormatter;
    private readonly IImmutableDictionary<(TenantEventType, uint), ITenantActivityFactory> _tenantActivityFactories;

    public TenantService(
        ILogger<TenantService> logger,
        ITaxBeaconDbContext context,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService,
        IEnumerable<IListToFileConverter> listToFileConverters,
        IDateTimeFormatter dateTimeFormatter,
        IEnumerable<ITenantActivityFactory> tenantActivityFactories)
    {
        _logger = logger;
        _context = context;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
        _dateTimeFormatter = dateTimeFormatter;
        _tenantActivityFactories = tenantActivityFactories?.ToImmutableDictionary(x => (x.EventType, x.Revision))
                                   ?? ImmutableDictionary<(TenantEventType, uint), ITenantActivityFactory>.Empty;
    }

    public async Task<QueryablePaging<TenantDto>> GetTenantsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) => await _context
        .Tenants
        .ProjectToType<TenantDto>()
        .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

    public async Task<byte[]> ExportTenantsAsync(FileType fileType,
        CancellationToken cancellationToken)
    {
        var exportTenants = await _context
            .Tenants
            .AsNoTracking()
            .ProjectToType<TenantExportModel>()
            .ToListAsync(cancellationToken);

        exportTenants.ForEach(t => t.CreatedDateView = _dateTimeFormatter.FormatDate(t.CreatedDateTimeUtc));

        _logger.LogInformation("{dateTime} - Tenants export was executed by {@userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(exportTenants);
    }

    public async Task<OneOf<TenantDto, NotFound>> GetTenantByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants.SingleOrDefaultAsync(t => t.Id == id, cancellationToken);

        return tenant is null ? new NotFound() : tenant.Adapt<TenantDto>();
    }

    public async Task<OneOf<ActivityDto, NotFound>> GetActivityHistoryAsync(Guid id, int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tenant is null)
        {
            return new NotFound();
        }

        var tenantActivityLogsQuery = _context.TenantActivityLogs.Where(log => log.TenantId == id);

        var count = await tenantActivityLogsQuery.CountAsync(cancellationToken);

        var pageCount = (uint)Math.Ceiling((double)count / pageSize);

        var activities = await tenantActivityLogsQuery
            .OrderByDescending(log => log.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ActivityDto(pageCount,
            activities.Select(x => _tenantActivityFactories[(x.EventType, x.Revision)].Create(x.Event)).ToList());
    }

    public async Task<OneOf<TenantDto, NotFound>> UpdateTenantAsync(Guid id, UpdateTenantDto updateTenantDto,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tenant is null)
        {
            return new NotFound();
        }

        var previousValues = JsonSerializer.Serialize(tenant.Adapt<UpdateTenantDto>());
        var currentUserInfo = _currentUserService.UserInfo;
        var eventDateTime = _dateTimeService.UtcNow;

        await _context.TenantActivityLogs.AddAsync(new TenantActivityLog
        {
            TenantId = id,
            Date = eventDateTime,
            Revision = 1,
            EventType = TenantEventType.TenantUpdatedEvent,
            Event = JsonSerializer.Serialize(new TenantUpdatedEvent(
                _currentUserService.UserId,
                currentUserInfo.Roles,
                currentUserInfo.FullName,
                eventDateTime,
                previousValues,
                JsonSerializer.Serialize(updateTenantDto)))
        }, cancellationToken);

        updateTenantDto.Adapt(tenant);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Tenant ({tenantId}) was updated by {@userId}",
            eventDateTime,
            id,
            _currentUserService.UserId);

        return tenant.Adapt<TenantDto>();
    }

    public async Task SwitchToTenantAsync(Guid? oldTenantId, Guid? newTenantId,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;

        var currentUser = await _context
            .Users
            .Where(u => u.Id == currentUserId && u.UserRoles.Any(ur => ur.Role.Name == Roles.SuperAdmin))
            .Select(u => new { u.FullName, Roles = string.Join(", ", u.UserRoles.Select(r => r.Role.Name)) })
            .SingleAsync(cancellationToken);

        var now = _dateTimeService.UtcNow;

        if (oldTenantId != null)
        {
            var item = new TenantActivityLog
            {
                EventType = TenantEventType.TenantExitedEvent,
                Event = JsonSerializer.Serialize(
                    new TenantExitedEvent(currentUserId,
                        currentUser.Roles,
                        currentUser.FullName,
                        now)),
                TenantId = oldTenantId.Value,
                Date = now,
                Revision = 1,
            };

            await _context.TenantActivityLogs.AddAsync(item, cancellationToken);
        }

        if (newTenantId != null)
        {
            // Ensuring that events appear in correct order (since we currently don't have an autoincrement field in the table
            now += TimeSpan.FromMilliseconds(1);

            var item = new TenantActivityLog
            {
                EventType = TenantEventType.TenantEnteredEvent,
                Event = JsonSerializer.Serialize(
                    new TenantEnteredEvent(currentUserId,
                        currentUser.Roles,
                        currentUser.FullName,
                        now)),
                TenantId = newTenantId.Value,
                Date = now,
                Revision = 1,
            };

            await _context.TenantActivityLogs.AddAsync(item, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "{dateTime} - User ({userId}) has switched from tenant ({oldTenantId}) to tenant ({newTenantId})",
            now,
            currentUserId,
            oldTenantId,
            newTenantId);
    }

    public async Task<OneOf<Success, NotFound>> ToggleDivisionsAsync(bool divisionEnabled,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants
            .SingleOrDefaultAsync(t => t.Id == _currentUserService.TenantId, cancellationToken);

        if (tenant is null)
        {
            return new NotFound();
        }

        tenant.DivisionEnabled = divisionEnabled;
        await _context.SaveChangesAsync(cancellationToken);

        return new Success();
    }

    public async Task<OneOf<List<AssignedTenantProgramDto>, NotFound>> GetTenantProgramsAsync(Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.Tenants.AnyAsync(t => t.Id == tenantId, cancellationToken))
        {
            return new NotFound();
        }

        return await _context.TenantsPrograms
            .Where(tp => tp.TenantId == tenantId && tp.IsDeleted == false)
            .Select(tp => tp.Program)
            .ProjectToType<AssignedTenantProgramDto>()
            .ToListAsync(cancellationToken);
    }

    public async Task<OneOf<Success, NotFound>> ChangeTenantProgramsAsync(Guid tenantId,
        IEnumerable<Guid> programsIds,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.Tenants.AnyAsync(t => t.Id == tenantId, cancellationToken))
        {
            return new NotFound();
        }

        var existingPrograms = await _context.TenantsPrograms
            .Where(tp => tp.TenantId == tenantId)
            .ToArrayAsync(cancellationToken);

        var currentUserInfo = _currentUserService.UserInfo;
        var assignedProgramsIds = await AssignProgramsAsync(tenantId, existingPrograms, programsIds, currentUserInfo, cancellationToken);
        var unassignProgramsIds = await UnassignProgramsAsync(tenantId, existingPrograms, programsIds, currentUserInfo, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        if (assignedProgramsIds.Any())
        {
            _logger.LogInformation("{dateTime} - The tenant ({tenantId}) has been assigned programs: {programsIds} by the user({userId})",
                _dateTimeService.UtcNow,
                tenantId,
                assignedProgramsIds,
                _currentUserService.UserId);
        }

        if (unassignProgramsIds.Any())
        {
            _logger.LogInformation("{dateTime} - The tenant ({tenantId}) has been unassigned programs: {programsIds} by the user({userId})",
                _dateTimeService.UtcNow,
                tenantId,
                unassignProgramsIds,
                _currentUserService.UserId);
        }

        return new Success();
    }

    private async Task<Guid[]> AssignProgramsAsync(Guid tenantId,
        IEnumerable<TenantProgram> existingPrograms,
        IEnumerable<Guid> newProgramsIds,
        (string currentUserFullName, string currentUserRoles) currentUserInfo,
        CancellationToken cancellationToken)
    {
        var addedProgramsIds = newProgramsIds
            .Except(existingPrograms.Select(ep => ep.ProgramId))
            .ToArray();

        var returnedPrograms = existingPrograms
            .Where(p => newProgramsIds.Contains(p.ProgramId) && p.IsDeleted == true)
            .ToArray();

        foreach (var returnedProgram in returnedPrograms)
        {
            returnedProgram.IsDeleted = false;
            returnedProgram.DeletedDateTimeUtc = null;
        }

        await _context.TenantsPrograms.AddRangeAsync(
            addedProgramsIds.Select(id => new TenantProgram
            {
                TenantId = tenantId,
                ProgramId = id,
                Status = Status.Active,
                IsDeleted = false
            }),
            cancellationToken);
        _context.TenantsPrograms.UpdateRange(returnedPrograms);

        var resultProgramsIds = returnedPrograms
            .Select(p => p.ProgramId)
            .Concat(addedProgramsIds)
            .ToArray();

        if (resultProgramsIds.Any())
        {
            var currentDate = _dateTimeService.UtcNow;
            await _context.TenantActivityLogs.AddAsync(new TenantActivityLog
            {
                TenantId = tenantId,
                Revision = 1,
                Date = currentDate,
                EventType = TenantEventType.TenantAssignProgramsEvent,
                Event = JsonSerializer.Serialize(
                    new TenantAssignProgramsEvent(
                        _currentUserService.UserId,
                        currentUserInfo.currentUserRoles,
                        currentUserInfo.currentUserFullName,
                        string.Join(", ", await _context.Programs
                            .Where(p => resultProgramsIds.Contains(p.Id))
                            .Select(p => p.Name)
                            .ToListAsync(cancellationToken)),
                        currentDate
                    ))
            }, cancellationToken);
        }

        return resultProgramsIds;
    }

    private async Task<Guid[]> UnassignProgramsAsync(Guid tenantId,
        IEnumerable<TenantProgram> existingPrograms,
        IEnumerable<Guid> newProgramsIds,
        (string currentUserFullName, string currentUserRoles) currentUserInfo,
        CancellationToken cancellationToken)
    {
        var deletedProgramsIds = existingPrograms
            .Where(tp => !newProgramsIds.Contains(tp.ProgramId) && tp.IsDeleted == false)
            .Select(tp => tp.ProgramId)
            .ToArray();

        _context.TenantsPrograms.RemoveRange(existingPrograms.Where(tp => deletedProgramsIds.Contains(tp.ProgramId)));

        if (deletedProgramsIds.Any())
        {
            var currentDate = _dateTimeService.UtcNow;
            await _context.TenantActivityLogs.AddAsync(new TenantActivityLog
            {
                TenantId = tenantId,
                Revision = 1,
                Date = currentDate,
                EventType = TenantEventType.TenantUnassignProgramEvent,
                Event = JsonSerializer.Serialize(
                    new TenantUnassignProgramsEvent(
                        _currentUserService.UserId,
                        currentUserInfo.currentUserRoles,
                        currentUserInfo.currentUserFullName,
                        string.Join(", ", await _context.Programs
                            .Where(p => deletedProgramsIds.Contains(p.Id))
                            .Select(p => p.Name)
                            .ToListAsync(cancellationToken)),
                        currentDate
                    ))
            }, cancellationToken);
        }

        return deletedProgramsIds;
    }
}
