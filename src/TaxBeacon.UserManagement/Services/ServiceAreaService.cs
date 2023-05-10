using Gridify;
using Gridify.EntityFramework;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using System.Text.Json;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities.ServiceArea;
using TaxBeacon.UserManagement.Models.Export;
using TaxBeacon.UserManagement.Services.Activities.ServiceArea;

namespace TaxBeacon.UserManagement.Services;

public class ServiceAreaService: IServiceAreaService
{
    private readonly ILogger<ServiceAreaService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeFormatter _dateTimeFormatter;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
    private readonly IImmutableDictionary<(ServiceAreaEventType, uint), IServiceAreaActivityFactory> _activityFactories;

    public ServiceAreaService(
        ILogger<ServiceAreaService> logger,
        ITaxBeaconDbContext context,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService,
        IDateTimeFormatter dateTimeFormatter,
        IEnumerable<IListToFileConverter> listToFileConverters,
        IEnumerable<IServiceAreaActivityFactory> serviceAreaActivityFactories)
    {
        _logger = logger;
        _context = context;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
        _dateTimeFormatter = dateTimeFormatter;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
        _activityFactories = serviceAreaActivityFactories?.ToImmutableDictionary(x => (x.EventType, x.Revision))
                            ?? ImmutableDictionary<(ServiceAreaEventType, uint), IServiceAreaActivityFactory>.Empty;
    }

    public async Task<OneOf<QueryablePaging<ServiceAreaDto>, NotFound>> GetServiceAreasAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
    {
        var serviceAreas = await _context
            .ServiceAreas
            .Where(d => d.TenantId == _currentUserService.TenantId)
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

        if (gridifyQuery.Page == 1 || serviceAreas.Query.Any())
        {
            return serviceAreas;
        }

        return new NotFound();
    }

    public async Task<byte[]> ExportServiceAreasAsync(FileType fileType,
        CancellationToken cancellationToken)
    {
        var exportServiceAreas = await _context
            .ServiceAreas
            .AsNoTracking()
            .Where(sa => sa.TenantId == _currentUserService.TenantId)
            .Select(sa => new ServiceAreaExportModel
            {
                Name = sa.Name,
                Description = sa.Description,
                Department = sa.Department == null ? string.Empty : sa.Department.Name,
                CreatedDateTimeUtc = sa.CreatedDateTimeUtc,
                AssignedUsersCount = sa.Users.Count()
            })
            .OrderBy(sa => sa.Name)
            .ToListAsync(cancellationToken);

        exportServiceAreas.ForEach(sa => sa.CreatedDateView = _dateTimeFormatter.FormatDate(sa.CreatedDateTimeUtc));

        _logger.LogInformation("{dateTime} - Service Areas export was executed by {@userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(exportServiceAreas);
    }

    public async Task<OneOf<ServiceAreaDetailsDto, NotFound>> GetServiceAreaDetailsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var serviceArea = await _context.ServiceAreas.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return serviceArea is null || serviceArea.TenantId != _currentUserService.TenantId
            ? new NotFound()
            : serviceArea.Adapt<ServiceAreaDetailsDto>();
    }

    public async Task<OneOf<ServiceAreaDetailsDto, NotFound>> UpdateServiceAreaDetailsAsync(Guid id,
        UpdateServiceAreaDto updateServiceAreaDto,
        CancellationToken cancellationToken)
    {
        var serviceArea = await _context.ServiceAreas.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (serviceArea is null)
        {
            return new NotFound();
        }

        var (userFullName, userRoles) = _currentUserService.UserInfo;
        var previousValues = JsonSerializer.Serialize(serviceArea.Adapt<UpdateServiceAreaDto>());

        var eventDateTime = _dateTimeService.UtcNow;

        await _context.ServiceAreaActivityLogs.AddAsync(new ServiceAreaActivityLog
        {
            TenantId = serviceArea.TenantId,
            ServiceAreaId = serviceArea.Id,
            Date = eventDateTime,
            Revision = 1,
            EventType = ServiceAreaEventType.ServiceAreaUpdatedEvent,
            Event = JsonSerializer.Serialize(new ServiceAreaUpdatedEvent(
                _currentUserService.UserId,
                userRoles ?? string.Empty,
                userFullName,
                eventDateTime,
                previousValues,
                JsonSerializer.Serialize(updateServiceAreaDto)))
        }, cancellationToken);

        updateServiceAreaDto.Adapt(serviceArea);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Service Area ({serviceArea}) was updated by {@userId}",
            eventDateTime,
            id,
            _currentUserService.UserId);

        return serviceArea.Adapt<ServiceAreaDetailsDto>();
    }

    public async Task<OneOf<ActivityDto, NotFound>> GetActivityHistoryAsync(Guid id, int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var serviceArea = await _context.ServiceAreas
            .FirstOrDefaultAsync(sa => sa.Id == id && sa.TenantId == _currentUserService.TenantId,
                cancellationToken);

        if (serviceArea is null)
        {
            return new NotFound();
        }

        var activityLogs = _context.ServiceAreaActivityLogs.Where(log => log.ServiceAreaId == id);

        var count = await activityLogs.CountAsync(cancellationToken);

        var pageCount = (uint)Math.Ceiling((double)count / pageSize);

        var activities = await activityLogs
            .OrderByDescending(log => log.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ActivityDto(pageCount,
            activities.Select(x => _activityFactories[(x.EventType, x.Revision)].Create(x.Event)).ToList());
    }

    public async Task<OneOf<QueryablePaging<ServiceAreaUserDto>, NotFound>> GetUsersAsync(Guid serviceAreaId, GridifyQuery gridifyQuery, CancellationToken cancellationToken)
    {
        var currentTenantId = _currentUserService.TenantId;
        var serviceArea = await _context.ServiceAreas
            .FirstOrDefaultAsync(sa => sa.Id == serviceAreaId && sa.TenantId == currentTenantId,
                cancellationToken);

        if (serviceArea is null)
        {
            return new NotFound();
        }

        var users = await _context
            .Users
            .AsNoTracking()
            .Where(sa => sa.TenantUsers.Any(x => x.TenantId == currentTenantId) && sa.ServiceAreaId == serviceAreaId)
            .Select(d => new ServiceAreaUserDto()
            {
                Id = d.Id,
                FullName = d.FullName,
                Email = d.Email,
                Team = d.Team != null ? d.Team.Name : string.Empty,
                JobTitle = d.JobTitle != null ? d.JobTitle.Name : string.Empty,
            })
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

        return users;
    }
}
