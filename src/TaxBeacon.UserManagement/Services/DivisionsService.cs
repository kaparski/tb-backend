using Gridify;
using Mapster;
using Microsoft.Extensions.Logging;
using OneOf.Types;
using OneOf;
using System.Collections.Immutable;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models.Activities.DivisionsActivities;
using TaxBeacon.UserManagement.Services.Activities.DivisionActivityHistory;

namespace TaxBeacon.UserManagement.Services
{
    public class DivisionsService: IDivisionsService
    {
        private readonly ILogger<DivisionsService> _logger;
        private readonly ITaxBeaconDbContext _context;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
        private readonly IDateTimeFormatter _dateTimeFormatter;
        private readonly IImmutableDictionary<(DivisionEventType, uint), IDivisionActivityFactory> _divisionActivityFactories;

        public DivisionsService(ILogger<DivisionsService> logger,
            ITaxBeaconDbContext context,
            IDateTimeService dateTimeService,
            ICurrentUserService currentUserService,
            IEnumerable<IListToFileConverter> listToFileConverters,
            IDateTimeFormatter dateTimeFormatter,
            IEnumerable<IDivisionActivityFactory> divisionActivityFactories)
        {
            _logger = logger;
            _context = context;
            _dateTimeService = dateTimeService;
            _currentUserService = currentUserService;
            _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                    ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
            _dateTimeFormatter = dateTimeFormatter;
            _divisionActivityFactories = divisionActivityFactories?.ToImmutableDictionary(x => (x.EventType, x.Revision))
                                         ?? ImmutableDictionary<(DivisionEventType, uint), IDivisionActivityFactory>.Empty;
        }

        public async Task<OneOf<QueryablePaging<DivisionDto>, NotFound>> GetDivisionsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
        {
            var tenantId = _currentUserService.TenantId;
            var divisions = await _context
                .Divisions
                .Where(div => div.TenantId == tenantId)
                .Select(div => new DivisionDto
                {
                    Id = div.Id,
                    Name = div.Name,
                    Description = div.Description,
                    CreatedDateTimeUtc = div.CreatedDateTimeUtc,
                    NumberOfUsers = div.Users.Count(),
                    Departments = string.Join(", ", div.Departments.Select(dep => dep.Name)),
                    Department = div.Departments.Select(dep => dep.Name)
                    .GroupBy(dep => 1)
                    .Select(g => string.Join(string.Empty, g.Select(s => "|" + s + "|")))
                    .FirstOrDefault() ?? string.Empty
                })
                .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

            if (gridifyQuery.Page == 1 || divisions.Query.Any())
            {
                return divisions;
            }

            return new NotFound();
        }

        public async Task<byte[]> ExportDivisionsAsync(FileType fileType,
        CancellationToken cancellationToken)
        {
            var tenantId = _currentUserService.TenantId;
            var exportTenants = await _context
                .Divisions
                .Where(d => d.TenantId == tenantId)
                .Select(div => new DivisionDto
                {
                    Id = div.Id,
                    Name = div.Name,
                    Description = div.Description,
                    CreatedDateTimeUtc = div.CreatedDateTimeUtc,
                    NumberOfUsers = div.Users.Count(),
                    Departments = string.Join(", ", div.Departments.Select(dep => dep.Name)),
                })
                .AsNoTracking()
                .ProjectToType<DivisionExportModel>()
                .ToListAsync(cancellationToken);

            exportTenants.ForEach(t => t.CreatedDateView = _dateTimeFormatter.FormatDate(t.CreatedDateTimeUtc));

            _logger.LogInformation("{dateTime} - Tenants export was executed by {@userId}",
                _dateTimeService.UtcNow,
                _currentUserService.UserId);

            return _listToFileConverters[fileType].Convert(exportTenants);
        }

        public async Task<OneOf<DivisionActivityDto, NotFound>> GetActivitiesAsync(Guid divisionId, uint page = 1,
            uint pageSize = 10, CancellationToken cancellationToken = default)
        {
            page = page == 0 ? 1 : page;
            pageSize = pageSize == 0 ? 10 : pageSize;

            var user = await _context.Users
                .Where(u => u.Id == divisionId && u.TenantUsers.Any(tu => tu.TenantId == _currentUserService.TenantId))
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return new NotFound();
            }

            var userActivitiesQuery = _context.DivisionActivityLogs
                .Where(ua => ua.DivisionId == divisionId && ua.TenantId == _currentUserService.TenantId);

            var count = await userActivitiesQuery.CountAsync(cancellationToken: cancellationToken);

            var pageCount = (uint)Math.Ceiling((double)count / pageSize);

            var activities = await userActivitiesQuery
                .OrderByDescending(x => x.Date)
                .Skip((int)((page - 1) * pageSize))
                .Take((int)pageSize)
                .ToListAsync(cancellationToken);

            return new DivisionActivityDto(pageCount,
                activities.Select(x => _divisionActivityFactories[(x.EventType, x.Revision)].Create(x.Event)).ToList());
        }

        public async Task<OneOf<DivisionDetailsDto, NotFound>> GetDivisionDetails(Guid divisionId, CancellationToken cancellationToken = default)
        {
            var divisions = await _context
                .Divisions
                .Include(x => x.Departments)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == _currentUserService.TenantId && x.Id == divisionId, cancellationToken);

            return divisions is null
                ? new NotFound()
                : divisions.Adapt<DivisionDetailsDto>();
        }
    }
}
