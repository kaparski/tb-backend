using Gridify;
using Gridify.EntityFramework;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Export;

namespace TaxBeacon.UserManagement.Services;

public class TeamService: ITeamService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TeamService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeFormatter _dateTimeFormatter;
    private readonly IDateTimeService _dateTimeService;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;

    public TeamService(ICurrentUserService currentUserService, ILogger<TeamService> logger, ITaxBeaconDbContext context, IDateTimeFormatter dateTimeFormatter, IDateTimeService dateTimeService, IEnumerable<IListToFileConverter> listToFileConverters)
    {
        _currentUserService = currentUserService;
        _logger = logger;
        _context = context;
        _dateTimeFormatter = dateTimeFormatter;
        _dateTimeService = dateTimeService;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
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
            .ProjectToType<TeamDto>()
            .AsNoTracking()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

        if (gridifyQuery.Page == 1 || teams.Query.Any())
        {
            return teams;
        }

        return new NotFound();
    }

    public async Task<byte[]> ExportTeamsAsync(FileType fileType, CancellationToken cancellationToken)
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
}
