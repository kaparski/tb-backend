using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using System.Text.Json;
using TaxBeacon.Administration.ServiceAreas.Activities.Factories;
using TaxBeacon.Administration.ServiceAreas.Activities.Models;
using TaxBeacon.Administration.ServiceAreas.Models;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.ServiceAreas;

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

    public IQueryable<ServiceAreaDto> QueryServiceAreas()
    {
        var items = _context.ServiceAreas.Where(d => d.TenantId == _currentUserService.TenantId);

        var itemDtos = items.Select(d => new ServiceAreaDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            CreatedDateTimeUtc = d.CreatedDateTimeUtc,
            AssignedUsersCount = d.Users.Count(u => u.TenantUsers.Any(x => x.TenantId == _currentUserService.TenantId)),
            DepartmentId = d.DepartmentId,
            Department = d.Department == null ? null : d.Department.Name
        });

        return itemDtos;
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
                AssignedUsersCount = sa.Users.Count(u => u.TenantUsers.Any(x => x.TenantId == _currentUserService.TenantId))
            })
            .OrderBy(sa => sa.Name)
            .ToListAsync(cancellationToken);

        exportServiceAreas.ForEach(sa => sa.CreatedDateView = _dateTimeFormatter.FormatDateTime(sa.CreatedDateTimeUtc));

        _logger.LogInformation("{dateTime} - Service Areas export was executed by {userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(exportServiceAreas);
    }

    public async Task<OneOf<ServiceAreaDetailsDto, NotFound>> GetServiceAreaDetailsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var serviceArea = await _context.ServiceAreas
            .Include(sa => sa.Department)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

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

        if (updateServiceAreaDto.DepartmentId != Guid.Empty)
        {
            var departmentToAdd = await _context.Departments.FirstOrDefaultAsync(d => d.Id == updateServiceAreaDto.DepartmentId);

            if (departmentToAdd != null && departmentToAdd.TenantId != serviceArea.TenantId)
            {
                return new NotFound();
            }
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

        _logger.LogInformation("{dateTime} - Service Area ({serviceArea}) was updated by {userId}",
            eventDateTime,
            id,
            _currentUserService.UserId);

        return await GetServiceAreaDetailsByIdAsync(id, cancellationToken);
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

    public async Task<IQueryable<ServiceAreaUserDto>> QueryUsersAsync(Guid serviceAreaId)
    {
        var currentTenantId = _currentUserService.TenantId;

        if ((await _context.ServiceAreas
            .FirstOrDefaultAsync(sa => sa.Id == serviceAreaId && sa.TenantId == currentTenantId)) is null)
        {
            throw new NotFoundException($"Service Area {serviceAreaId} not found");
        }

        var users = _context
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
        ;

        return users;
    }
}
