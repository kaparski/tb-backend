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

namespace TaxBeacon.UserManagement.Services;

public class TenantService: ITenantService
{
    private readonly ILogger<TenantService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
    private readonly IDateTimeFormatter _dateTimeFormatter;

    public TenantService(
        ILogger<TenantService> logger,
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

    public async Task<OneOf<TenantDto, NotFound>> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken)
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
}
