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
using TaxBeacon.UserManagement.Models.Activities.JobTitle;
using TaxBeacon.UserManagement.Models.Export;
using TaxBeacon.UserManagement.Services.Activities.JobTitle;

namespace TaxBeacon.UserManagement.Services;

public class JobTitleService: IJobTitleService
{
    private readonly ILogger<JobTitleService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeFormatter _dateTimeFormatter;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
    private readonly IImmutableDictionary<(JobTitleEventType, uint), IJobTitleActivityFactory> _activityFactories;

    public JobTitleService(
        ILogger<JobTitleService> logger,
        ITaxBeaconDbContext context,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService,
        IDateTimeFormatter dateTimeFormatter,
        IEnumerable<IListToFileConverter> listToFileConverters,
        IEnumerable<IJobTitleActivityFactory> jobTitleActivityFactories)
    {
        _logger = logger;
        _context = context;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
        _dateTimeFormatter = dateTimeFormatter;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
        _activityFactories = jobTitleActivityFactories?.ToImmutableDictionary(x => (x.EventType, x.Revision))
                            ?? ImmutableDictionary<(JobTitleEventType, uint), IJobTitleActivityFactory>.Empty;
    }

    public IQueryable<JobTitleDto> QueryJobTitles()
    {
        var items = _context.JobTitles.Where(d => d.TenantId == _currentUserService.TenantId);

        var itemDtos = items.Select(d => new JobTitleDto()
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

    public async Task<QueryablePaging<JobTitleDto>> GetJobTitlesAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) =>
        await _context
            .JobTitles
            .Where(d => d.TenantId == _currentUserService.TenantId)
            .Select(d => new JobTitleDto()
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                CreatedDateTimeUtc = d.CreatedDateTimeUtc,
                AssignedUsersCount = d.Users.Count(u => u.TenantUsers.Any(x => x.TenantId == _currentUserService.TenantId)),
                Department = d.Department == null ? string.Empty : d.Department.Name
            })
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

    public async Task<byte[]> ExportJobTitlesAsync(FileType fileType,
        CancellationToken cancellationToken)
    {
        var exportJobTitles = await _context
            .JobTitles
            .AsNoTracking()
            .Where(sa => sa.TenantId == _currentUserService.TenantId)
            .Select(sa => new JobTitleExportModel
            {
                Name = sa.Name,
                Description = sa.Description,
                Department = sa.Department == null ? string.Empty : sa.Department.Name,
                CreatedDateTimeUtc = sa.CreatedDateTimeUtc,
                AssignedUsersCount = sa.Users.Count(u => u.TenantUsers.Any(x => x.TenantId == _currentUserService.TenantId))
            })
            .OrderBy(sa => sa.Name)
            .ToListAsync(cancellationToken);

        exportJobTitles.ForEach(sa => sa.CreatedDateView = _dateTimeFormatter.FormatDate(sa.CreatedDateTimeUtc));

        _logger.LogInformation("{dateTime} - Job Titles export was executed by {@userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(exportJobTitles);
    }

    public async Task<OneOf<JobTitleDetailsDto, NotFound>> GetJobTitleDetailsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var jobTitle = await _context.JobTitles.Include(jt => jt.Department).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return jobTitle is null || jobTitle.TenantId != _currentUserService.TenantId
            ? new NotFound()
            : jobTitle.Adapt<JobTitleDetailsDto>();
    }

    public async Task<OneOf<JobTitleDetailsDto, NotFound>> UpdateJobTitleDetailsAsync(Guid id,
        UpdateJobTitleDto updateJobTitleDto,
        CancellationToken cancellationToken)
    {
        var jobTitle = await _context.JobTitles.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (jobTitle is null)
        {
            return new NotFound();
        }

        if (updateJobTitleDto.DepartmentId != Guid.Empty)
        {
            var departmentToAdd = await _context.Departments.FirstOrDefaultAsync(d => d.Id == updateJobTitleDto.DepartmentId);

            if (departmentToAdd != null && departmentToAdd.TenantId != jobTitle.TenantId)
            {
                return new NotFound();
            }
        }

        var (userFullName, userRoles) = _currentUserService.UserInfo;
        var previousValues = JsonSerializer.Serialize(jobTitle.Adapt<UpdateJobTitleDto>());

        var eventDateTime = _dateTimeService.UtcNow;

        await _context.JobTitleActivityLogs.AddAsync(new JobTitleActivityLog
        {
            TenantId = jobTitle.TenantId,
            JobTitleId = jobTitle.Id,
            Date = eventDateTime,
            Revision = 1,
            EventType = JobTitleEventType.JobTitleUpdatedEvent,
            Event = JsonSerializer.Serialize(new JobTitleUpdatedEvent(
                _currentUserService.UserId,
                userRoles ?? string.Empty,
                userFullName,
                eventDateTime,
                previousValues,
                JsonSerializer.Serialize(updateJobTitleDto)))
        }, cancellationToken);

        updateJobTitleDto.Adapt(jobTitle);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Job Title ({jobTitle}) was updated by {@userId}",
            eventDateTime,
            id,
            _currentUserService.UserId);

        return await GetJobTitleDetailsByIdAsync(id, cancellationToken);
    }

    public async Task<OneOf<ActivityDto, NotFound>> GetActivityHistoryAsync(Guid id, int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var jobTitle = await _context.JobTitles
            .FirstOrDefaultAsync(sa => sa.Id == id && sa.TenantId == _currentUserService.TenantId,
                cancellationToken);

        if (jobTitle is null)
        {
            return new NotFound();
        }

        var activityLogs = _context.JobTitleActivityLogs.Where(log => log.JobTitleId == id);

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

    public async Task<OneOf<QueryablePaging<JobTitleUserDto>, NotFound>> GetUsersAsync(Guid jobTitleId, GridifyQuery gridifyQuery, CancellationToken cancellationToken)
    {
        var currentTenantId = _currentUserService.TenantId;
        var jobTitle = await _context.JobTitles
            .FirstOrDefaultAsync(sa => sa.Id == jobTitleId && sa.TenantId == currentTenantId,
                cancellationToken);

        if (jobTitle is null)
        {
            return new NotFound();
        }

        var users = await _context
            .Users
            .AsNoTracking()
            .Where(sa => sa.TenantUsers.Any(x => x.TenantId == currentTenantId) && sa.JobTitleId == jobTitleId)
            .Select(d => new JobTitleUserDto()
            {
                Id = d.Id,
                FullName = d.FullName,
                Email = d.Email,
                Department = d.Department != null ? d.Department.Name : string.Empty,
                ServiceArea = d.ServiceArea != null ? d.ServiceArea.Name : string.Empty,
                Team = d.Team != null ? d.Team.Name : string.Empty,
            })
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

        return users;
    }
}
