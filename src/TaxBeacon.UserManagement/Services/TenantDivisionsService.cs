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

namespace TaxBeacon.UserManagement.Services
{
    public class TenantDivisionsService: ITenantDivisionsService
    {
        private readonly ILogger<TenantService> _logger;
        private readonly ITaxBeaconDbContext _context;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
        private readonly IDateTimeFormatter _dateTimeFormatter;

        public TenantDivisionsService(ILogger<TenantService> logger,
        ITaxBeaconDbContext context,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService,
        IEnumerable<IListToFileConverter> listToFileConverters,
        IDateTimeFormatter dateTimeFormatter)
        {
            _logger = logger;
            _context = context;
            _dateTimeService = dateTimeService;
            _currentUserService = currentUserService;
            _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                    ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
            _dateTimeFormatter = dateTimeFormatter;
        }

        public async Task<OneOf<QueryablePaging<DivisionDto>, NotFound>> GetTenantDivisionsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
        {
            var divisions = await _context
                .Divisions
                .Select(d => new DivisionDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    CreatedDateTimeUtc = d.CreatedDateTimeUtc,
                    NumberOfUsers = d.Users.Count()
                })
                .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

            if (gridifyQuery.Page == 1 || divisions.Query.Any())
            {
                return divisions;
            }

            return new NotFound();
        }

        public async Task<byte[]> ExportTenantDivisionsAsync(FileType fileType,
        CancellationToken cancellationToken)
        {
            var exportTenants = await _context
                .Tenants
                .AsNoTracking()
                .ProjectToType<DivisionExportModel>()
                .ToListAsync(cancellationToken);

            exportTenants.ForEach(t => t.CreatedDateView = _dateTimeFormatter.FormatDate(t.CreatedDateTimeUtc));

            _logger.LogInformation("{dateTime} - Tenants export was executed by {@userId}",
                _dateTimeService.UtcNow,
                _currentUserService.UserId);

            return _listToFileConverters[fileType].Convert(exportTenants);
        }
    }
}
