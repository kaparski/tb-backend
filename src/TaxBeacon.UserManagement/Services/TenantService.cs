using Gridify;
using Gridify.EntityFramework;
using Mapster;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.Common.Converters;
using System.Collections.Immutable;
using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Services.Activities.Tenant;
using TaxBeacon.DAL.Entities;
using TaxBeacon.UserManagement.Models.Activities.Tenant;

namespace TaxBeacon.UserManagement.Services;

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

    public async Task<OneOf<QueryablePaging<TenantDto>, NotFound>> GetTenantsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
    {
        var tenants = await _context
            .Tenants
            .ProjectToType<TenantDto>()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

        if (gridifyQuery.Page == 1 || tenants.Query.Any())
        {
            return tenants;
        }

        return new NotFound();
    }

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

    public async Task<OneOf<TenantDto, NotFound>> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return tenant is null ? new NotFound() : tenant.Adapt<TenantDto>();
    }

    public async Task<OneOf<QueryablePaging<DepartmentDto>, NotFound>> GetDepartmentsAsync(Guid tenantId,
        GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
    {
        var departments = await _context
            .Departments
            .Where(d => d.TenantId == tenantId)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                CreatedDateTimeUtc = d.CreatedDateTimeUtc,
                AssignedUsersCount = d.Users.Count(),
                Division = d.Division == null ? string.Empty : d.Division.Name,
                ServiceAreas = string.Join(", ", d.ServiceAreas.Select(sa => sa.Name)),
                ServiceArea = d.ServiceAreas.Select(sa => sa.Name)
                    .GroupBy(sa => 1)
                    .Select(g => string.Join(string.Empty, g.Select(s => "|" + s + "|")))
                    .FirstOrDefault() ?? string.Empty
            })
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

        if (gridifyQuery.Page == 1 || departments.Query.Any())
        {
            return departments;
        }

        return new NotFound();
    }

    public async Task<byte[]> ExportDepartmentsAsync(Guid tenantId,
        FileType fileType,
        CancellationToken cancellationToken)
    {
        var exportDepartments = await _context
            .Departments
            .AsNoTracking()
            .Where(d => d.TenantId == tenantId)
            .Select(d => new DepartmentExportModel
            {
                Name = d.Name,
                Description = d.Description,
                Division = d.Division == null ? string.Empty : d.Division.Name,
                ServiceAreas = string.Join(", ", d.ServiceAreas.Select(sa => sa.Name)),
                CreatedDateTimeUtc = d.CreatedDateTimeUtc,
                AssignedUsersCount = d.Users.Count()
            })
            .ToListAsync(cancellationToken);

        exportDepartments.ForEach(t => t.CreatedDateView = _dateTimeFormatter.FormatDate(t.CreatedDateTimeUtc));

        _logger.LogInformation("{dateTime} - Departments export was executed by {@userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(exportDepartments);
    }

    public async Task<OneOf<QueryablePaging<ServiceAreaDto>, NotFound>> GetServiceAreasAsync(Guid tenantId,
        GridifyQuery gridifyQuery, CancellationToken cancellationToken = default)
    {
        var serviceAreas = await _context
            .ServiceAreas
            .Where(d => d.TenantId == tenantId)
            .Select(d => new ServiceAreaDto()
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                CreatedDateTimeUtc = d.CreatedDateTimeUtc,
                AssignedUsersCount = d.Users.Count(),
                Department = d.Department == null ? string.Empty : d.Department.Name
            })
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

        if (gridifyQuery.Page == 1 && serviceAreas.Query.Any())
        {
            return serviceAreas;
        }

        return new NotFound();
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
        var currentUserFullName = (await _context.Users.FindAsync(_currentUserService.UserId, cancellationToken))!.FullName;
        var currentUserRoles = await _context
            .TenantUserRoles
            .Where(x => x.UserId == _currentUserService.UserId && x.TenantId == _currentUserService.TenantId)
            .GroupBy(r => 1, t => t.TenantRole.Role.Name)
            .Select(group => string.Join(", ", group.Select(name => name)))
            .FirstOrDefaultAsync(cancellationToken);
        var eventDateTime = _dateTimeService.UtcNow;

        await _context.TenantActivityLogs.AddAsync(new TenantActivityLog
        {
            TenantId = id,
            Date = eventDateTime,
            Revision = 1,
            EventType = TenantEventType.TenantUpdatedEvent,
            Event = JsonSerializer.Serialize(new TenantUpdatedEvent(
                _currentUserService.UserId,
                currentUserRoles ?? string.Empty,
                currentUserFullName,
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

    public async Task SwitchToTenantAsync(Guid? oldTenantId, Guid? newTenantId, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;

        // TODO: use roleId instead of role name for SuperAdmin role
        var currentUser = await _context
            .Users
            .Where(u => u.Id == currentUserId && u.UserRoles.Any(ur => ur.Role.Name == "Super admin"))
            .Select(u => new
            {
                u.FullName,
                Roles = string.Join(", ", u.UserRoles.Select(r => r.Role.Name))
            })
            .SingleAsync();

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

        _logger.LogInformation("{dateTime} - User ({userId}) has switched from tenant ({oldTenantId}) to tenant ({newTenantId})",
            now,
            currentUserId,
            oldTenantId,
            newTenantId);
    }
}
