using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Erros;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public class TableFilterService: ITableFiltersService
{
    private readonly ILogger<TableFilterService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;

    public TableFilterService(ILogger<TableFilterService> logger,
        ITaxBeaconDbContext context,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _context = context;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
    }

    public async Task<OneOf<List<TableFilterDto>, NameAlreadyExists>> CreateFilterAsync(
        CreateTableFilterDto createTableFilterDto,
        CancellationToken cancellationToken = default)
    {
        Guid? tenantId = _currentUserService.TenantId != default
            ? _currentUserService.TenantId
            : null;

        if (await _context.TableFilters
                .AnyAsync(tf => tf.TenantId == tenantId
                                && tf.UserId == _currentUserService.UserId
                                && tf.Name == createTableFilterDto.Name, cancellationToken))
        {
            return new NameAlreadyExists();
        }

        var newTableFilter = new TableFilter
        {
            TenantId = tenantId,
            UserId = _currentUserService.UserId,
            Name = createTableFilterDto.Name,
            Configuration = createTableFilterDto.Configuration,
            TableType = createTableFilterDto.TableType
        };

        await _context.TableFilters.AddAsync(newTableFilter, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Filter ({filterId}) with name {filterName} was created by {@userId}",
            _dateTimeService.UtcNow,
            newTableFilter.Id,
            newTableFilter.Name,
            _currentUserService.UserId);

        return await GetFiltersAsync(createTableFilterDto.TableType, cancellationToken);
    }

    public async Task<OneOf<List<TableFilterDto>, NotFound>> DeleteFilterAsync(Guid filterId,
        CancellationToken cancellationToken = default)
    {
        var tableFilter = await _context.TableFilters.FirstOrDefaultAsync(tf => tf.Id == filterId, cancellationToken);

        if (tableFilter is null)
        {
            return new NotFound();
        }

        _context.TableFilters.Remove(tableFilter);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Filter ({filterId}) was deleted by {@userId}",
            _dateTimeService.UtcNow,
            tableFilter.Id,
            _currentUserService.UserId);

        return await GetFiltersAsync(tableFilter.TableType, cancellationToken);
    }

    public async Task<List<TableFilterDto>> GetFiltersAsync(EntityType tableType,
        CancellationToken cancellationToken = default)
    {
        Guid? tenantId = _currentUserService.TenantId != default
            ? _currentUserService.TenantId
            : null;

        return await _context.TableFilters
            .Where(tf =>
                tf.TenantId == tenantId
                && tf.TableType == tableType
                && tf.UserId == _currentUserService.UserId)
            .AsNoTracking()
            .ProjectToType<TableFilterDto>()
            .ToListAsync(cancellationToken);
    }
}
