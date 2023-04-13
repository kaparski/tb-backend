using Gridify;
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
    private readonly ILogger<UserService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeFormatter _dateTimeFormatter;
    private readonly IDateTimeService _dateTimeService;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;

    public TeamService(ICurrentUserService currentUserService, ILogger<UserService> logger, ITaxBeaconDbContext context, IDateTimeFormatter dateTimeFormatter, IDateTimeService dateTimeService, IImmutableDictionary<FileType, IListToFileConverter> listToFileConverters)
    {
        _currentUserService = currentUserService;
        _logger = logger;
        _context = context;
        _dateTimeFormatter = dateTimeFormatter;
        _dateTimeService = dateTimeService;
        _listToFileConverters = listToFileConverters;
    }

    public Task<OneOf<QueryablePaging<TeamDto>, NotFound>> GetTeamsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public async Task<byte[]> ExportTeamsAsync(FileType fileType, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        var exportTeams = await _context.Users
            .Where(u => u.TenantUsers.Any(tu => tu.TenantId == tenantId))
            .OrderBy(u => u.Email)
            .AsNoTracking()
            .ProjectToType<TeamExportModel>()
            .ToListAsync(cancellationToken);

        exportTeams.ForEach(x => x.CreatedDateView = _dateTimeFormatter.FormatDate(x.CreatedDateTimeUtc));

        _logger.LogInformation("{dateTime} - Teams export was executed by {@userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(exportTeams);
    }
}
