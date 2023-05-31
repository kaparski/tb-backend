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
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Services.Activities.Department;

namespace TaxBeacon.UserManagement.Services;

public class DepartmentService: IDepartmentService
{
    private readonly ILogger<DepartmentService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
    private readonly IDateTimeFormatter _dateTimeFormatter;
    private readonly IImmutableDictionary<(DepartmentEventType, uint), IDepartmentActivityFactory> _activityFactories;

    public DepartmentService(
        ILogger<DepartmentService> logger,
        ITaxBeaconDbContext context,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService,
        IEnumerable<IListToFileConverter> listToFileConverters,
        IDateTimeFormatter dateTimeFormatter,
        IEnumerable<IDepartmentActivityFactory> activityFactories)
    {
        _logger = logger;
        _context = context;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
        _dateTimeFormatter = dateTimeFormatter;
        _activityFactories = activityFactories?.ToImmutableDictionary(x => (x.EventType, x.Revision))
                                   ?? ImmutableDictionary<(DepartmentEventType, uint), IDepartmentActivityFactory>.Empty;
    }

    public async Task<QueryablePaging<DepartmentDto>> GetDepartmentsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) =>
        await _context
            .Departments
            .Where(d => d.TenantId == _currentUserService.TenantId)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                CreatedDateTimeUtc = d.CreatedDateTimeUtc,
                AssignedUsersCount = d.Users.Count(),
                Division = d.Division == null ? string.Empty : d.Division.Name,
                ServiceArea = d.ServiceAreas.Select(sa => sa.Name)
                    .GroupBy(sa => 1)
                    .Select(g => string.Join(string.Empty, g.Select(s => "|" + s + "|")))
                    .FirstOrDefault() ?? string.Empty
            })
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

    public async Task<byte[]> ExportDepartmentsAsync(FileType fileType,
        CancellationToken cancellationToken)
    {
        var exportDepartments = await _context
            .Departments
            .AsNoTracking()
            .Where(d => d.TenantId == _currentUserService.TenantId)
            .Select(d => new DepartmentExportModel
            {
                Name = d.Name,
                Description = d.Description,
                Division = d.Division == null ? string.Empty : d.Division.Name,
                ServiceAreas = string.Join(", ", d.ServiceAreas.Select(sa => sa.Name)),
                CreatedDateTimeUtc = d.CreatedDateTimeUtc,
                AssignedUsersCount = d.Users.Count()
            })
            .OrderBy(dep => dep.Name)
            .ToListAsync(cancellationToken);

        exportDepartments.ForEach(t => t.CreatedDateView = _dateTimeFormatter.FormatDate(t.CreatedDateTimeUtc));

        _logger.LogInformation("{dateTime} - Departments export was executed by {@userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(exportDepartments);
    }

    public async Task<OneOf<ActivityDto, NotFound>> GetActivityHistoryAsync(Guid id, int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DepartmentActivityLogs.Where(log => log.DepartmentId == id && log.TenantId == _currentUserService.TenantId);

        var count = await query.CountAsync(cancellationToken);

        var pageCount = (uint)Math.Ceiling((double)count / pageSize);

        var activities = await query
            .OrderByDescending(log => log.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ActivityDto(pageCount,
            activities.Select(x => _activityFactories[(x.EventType, x.Revision)].Create(x.Event)).ToList());
    }

    public async Task<OneOf<DepartmentDetailsDto, NotFound>> GetDepartmentDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _context
            .Departments
            .Include(x => x.Division)
            .Include(x => x.JobTitles.OrderBy(dep => dep.Name))
            .Include(x => x.ServiceAreas.OrderBy(dep => dep.Name))
            .Where(x => x.TenantId == _currentUserService.TenantId && x.Id == id)
            .AsNoTracking()
            .ProjectToType<DepartmentDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? new NotFound() : item;
    }

    public async Task<OneOf<DepartmentDetailsDto, NotFound, InvalidOperation>> UpdateDepartmentAsync(Guid id, UpdateDepartmentDto updatedEntity,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context
            .Departments
            .Include(dep => dep.Division)
            .Include(dep => dep.ServiceAreas)
            .Include(dep => dep.JobTitles)
            .SingleOrDefaultAsync(t => t.Id == id && t.TenantId == _currentUserService.TenantId, cancellationToken);

        if (entity is null)
        {
            return new NotFound();
        }

        if (updatedEntity.DivisionId != Guid.Empty)
        {
            var departmentToAdd = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == updatedEntity.DivisionId && d.TenantId != entity.TenantId, cancellationToken);

            if (departmentToAdd != null)
            {
                return new NotFound();
            }
        }

        var alreadyAssignedServiceAreas = await _context.ServiceAreas
                .Where(sa => updatedEntity.ServiceAreasIds.Contains(sa.Id)
                    && sa.DepartmentId != null
                    && sa.DepartmentId != entity.Id
                    && sa.TenantId == _currentUserService.TenantId)
                .Select(sa => sa.Name).ToListAsync(cancellationToken);

        if (alreadyAssignedServiceAreas.Any())
        {
            return new InvalidOperation($"Service area(s) {string.Join(", ", alreadyAssignedServiceAreas)} have been assigned to another department");
        }

        var alreadyAssignedJobTitles = await _context.JobTitles
               .Where(jt => updatedEntity.ServiceAreasIds.Contains(jt.Id)
                   && jt.DepartmentId != null
                   && jt.DepartmentId != entity.Id
                   && jt.TenantId == _currentUserService.TenantId)
               .Select(jt => jt.Name).ToListAsync(cancellationToken);

        if (alreadyAssignedJobTitles.Any())
        {
            return new InvalidOperation($"Job title(s) {string.Join(", ", alreadyAssignedJobTitles)} have been assigned to another department");
        }

        var eventDateTime = _dateTimeService.UtcNow;

        var previousValues = JsonSerializer.Serialize(entity.Adapt<UpdateDepartmentDto>());

        var (currentUserFullName, currentUserRoles) = _currentUserService.UserInfo;

        var curServiceAreasIds = entity.ServiceAreas.Select(sa => sa.Id).ToList();

        // Removes association with serviceAreas
        await _context.ServiceAreas
            .Where(sa => curServiceAreasIds.Except(updatedEntity.ServiceAreasIds).Contains(sa.Id))
            .ForEachAsync(sa => sa.DepartmentId = null, cancellationToken);

        // Set up association with freshly added service areas
        await _context.ServiceAreas
            .Where(sa => updatedEntity.ServiceAreasIds.Except(curServiceAreasIds).Contains(sa.Id))
            .ForEachAsync(sa => sa.DepartmentId = id, cancellationToken);

        var curJobTitlesIds = entity.JobTitles.Select(jt => jt.Id).ToList();

        // Removes association with job titles
        await _context.JobTitles
            .Where(jt => curJobTitlesIds.Except(updatedEntity.JobTitlesIds).Contains(jt.Id))
            .ForEachAsync(jt => jt.DepartmentId = null, cancellationToken);

        // Set up association with freshly added job titles
        await _context.JobTitles
            .Where(jt => updatedEntity.JobTitlesIds.Except(curJobTitlesIds).Contains(jt.Id))
            .ForEachAsync(jt => jt.DepartmentId = id, cancellationToken);

        await _context.DepartmentActivityLogs.AddAsync(new DepartmentActivityLog
        {
            DepartmentId = id,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = DepartmentEventType.DepartmentUpdatedEvent,
            Event = JsonSerializer.Serialize(new DepartmentUpdatedEvent(
                _currentUserService.UserId,
                currentUserRoles,
                currentUserFullName,
                eventDateTime,
                previousValues,
                JsonSerializer.Serialize(updatedEntity)))
        }, cancellationToken);

        entity.Name = updatedEntity.Name;
        entity.Description = updatedEntity.Description;
        entity.DivisionId = updatedEntity.DivisionId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Department ({departmentId}) was updated by {@userId}",
            eventDateTime,
            id,
            _currentUserService.UserId);

        return (await GetDepartmentDetailsAsync(id, cancellationToken)).AsT0;
    }

    public async Task<OneOf<QueryablePaging<DepartmentUserDto>, NotFound>> GetDepartmentUsersAsync(Guid departmentId, GridifyQuery gridifyQuery, CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUserService.TenantId;

        var entity = await _context
            .Departments
            .SingleOrDefaultAsync(t => t.Id == departmentId && t.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            return new NotFound();
        }

        var users = await _context
            .Users
            .Where(u => u.DepartmentId == departmentId && u.TenantUsers.Any(x => x.TenantId == tenantId && x.UserId == u.Id))
            .Select(u => new DepartmentUserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                JobTitle = u.JobTitle == null ? string.Empty : u.JobTitle.Name,
                ServiceArea = u.ServiceArea == null ? string.Empty : u.ServiceArea.Name,
                Team = u.Team == null ? string.Empty : u.Team.Name,
            })
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

        return users;
    }

    public async Task<OneOf<DepartmentServiceAreaDto[], NotFound>> GetDepartmentServiceAreasAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments.SingleOrDefaultAsync(
                t => t.Id == id && t.TenantId == _currentUserService.TenantId, cancellationToken);

        return department is null
            ? new NotFound()
            : await _context.ServiceAreas
                .Where(sa => sa.DepartmentId == id)
                .ProjectToType<DepartmentServiceAreaDto>()
                .ToArrayAsync(cancellationToken);
    }

    public async Task<OneOf<DepartmentJobTitleDto[], NotFound>> GetDepartmentJobTitlesAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments.SingleOrDefaultAsync(
            t => t.Id == id && t.TenantId == _currentUserService.TenantId, cancellationToken);

        return department is null
            ? new NotFound()
            : await _context.JobTitles
                .Where(sa => sa.DepartmentId == id)
                .ProjectToType<DepartmentJobTitleDto>()
                .ToArrayAsync(cancellationToken);
    }
}
