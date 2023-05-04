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
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Models.Export;
using TaxBeacon.UserManagement.Services.Activities;

namespace TaxBeacon.UserManagement.Services;

public class TeamService: ITeamService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TeamService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeFormatter _dateTimeFormatter;
    private readonly IDateTimeService _dateTimeService;
    private readonly IImmutableDictionary<(TeamEventType, uint), ITeamActivityFactory> _teamActivityFactories;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;

    public TeamService(
        ICurrentUserService currentUserService,
        ILogger<TeamService> logger,
        ITaxBeaconDbContext context,
        IDateTimeFormatter dateTimeFormatter,
        IDateTimeService dateTimeService,
        IEnumerable<IListToFileConverter> listToFileConverters,
        IEnumerable<ITeamActivityFactory> teamActivityFactories
        )
    {
        _currentUserService = currentUserService;
        _logger = logger;
        _context = context;
        _dateTimeFormatter = dateTimeFormatter;
        _dateTimeService = dateTimeService;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
        _teamActivityFactories = teamActivityFactories?.ToImmutableDictionary(x => (x.EventType, x.Revision))
                                        ?? ImmutableDictionary<(TeamEventType, uint), ITeamActivityFactory>.Empty;
    }

    public async Task<OneOf<QueryablePaging<TeamDto>, NotFound>> GetTeamsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
    {
        var currentTenantId = _currentUserService.TenantId;
        var teams = await _context
            .Teams
            .Where(x => x.TenantId == currentTenantId)
            .Select(t => new TeamDto()
            {
                Id = t.Id,
                Name = t.Name,
                NumberOfUsers = t.Users.Count,
                CreatedDateTimeUtc = t.CreatedDateTimeUtc,
                Description = t.Description
            })
            .AsNoTracking()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

        if (gridifyQuery.Page == 1 || teams.Query.Any())
        {
            return teams;
        }

        return new NotFound();
    }

    public async Task<byte[]> ExportTeamsAsync(FileType fileType, CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUserService.TenantId;

        var exportTeams = await _context.Teams
            .Where(t => t.TenantId == tenantId)
            .OrderBy(u => u.Name)
            .ProjectToType<TeamExportModel>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        exportTeams.ForEach(x => x.CreatedDateView = _dateTimeFormatter.FormatDate(x.CreatedDateTimeUtc));

        _logger.LogInformation("{dateTime} - Teams export was executed by {@userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(exportTeams);
    }

    public async Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid teamId, int page = 1,
    int pageSize = 10, CancellationToken cancellationToken = default)
    {
        page = page == 0 ? 1 : page;
        pageSize = pageSize == 0 ? 10 : pageSize;

        var team = await _context.Teams
            .Where(d => d.Id == teamId && d.TenantId == _currentUserService.TenantId)
            .FirstOrDefaultAsync(cancellationToken);

        if (team is null)
        {
            return new NotFound();
        }

        var teamActivityLogs = _context.TeamActivityLogs
            .Where(ua => ua.TeamId == teamId && ua.TenantId == _currentUserService.TenantId);

        var count = await teamActivityLogs.CountAsync(cancellationToken: cancellationToken);

        var pageCount = (uint)Math.Ceiling((double)count / pageSize);

        var activities = await teamActivityLogs
            .OrderByDescending(x => x.Date)
            .Skip((int)((page - 1) * pageSize))
            .Take((int)pageSize)
            .ToListAsync(cancellationToken);

        return new ActivityDto(pageCount,
            activities.Select(x => _teamActivityFactories[(x.EventType, x.Revision)].Create(x.Event)).ToList());
    }

    public async Task<OneOf<TeamDetailsDto, NotFound>> GetTeamDetailsAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await _context
            .Teams
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == _currentUserService.TenantId && x.Id == teamId, cancellationToken);

        return team is null
            ? new NotFound()
            : team.Adapt<TeamDetailsDto>();
    }

    public async Task<OneOf<TeamDto, NotFound>> UpdateTeamAsync(Guid id, UpdateTeamDto updateTeamDto,
CancellationToken cancellationToken = default)
    {
        var team = await _context
            .Teams
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == _currentUserService.TenantId, cancellationToken);

        if (team is null)
        {
            return new NotFound();
        }

        var previousValues = JsonSerializer.Serialize(team.Adapt<UpdateTeamDto>());
        var userInfo = _currentUserService.UserInfo;
        var eventDateTime = _dateTimeService.UtcNow;

        await _context.TeamActivityLogs.AddAsync(new TeamActivityLog
        {
            TeamId = id,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = TeamEventType.TeamUpdatedEvent,
            Event = JsonSerializer.Serialize(new TeamUpdatedEvent(
                _currentUserService.UserId,
                userInfo.Roles ?? string.Empty,
                userInfo.FullName,
                eventDateTime,
                previousValues,
                JsonSerializer.Serialize(updateTeamDto)))
        }, cancellationToken);

        updateTeamDto.Adapt(team);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Team ({teamId}) was updated by {@userId}",
            eventDateTime,
            id,
            _currentUserService.UserId);

        return team.Adapt<TeamDto>();
    }
}
