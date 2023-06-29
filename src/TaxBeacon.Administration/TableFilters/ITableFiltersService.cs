using OneOf;
using OneOf.Types;
using TaxBeacon.Administration.TableFilters.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Errors;

namespace TaxBeacon.Administration.TableFilters;

public interface ITableFiltersService
{
    Task<OneOf<List<TableFilterDto>, NameAlreadyExists>> CreateFilterAsync(CreateTableFilterDto createTableFilterDto,
        CancellationToken cancellationToken = default);

    Task<OneOf<List<TableFilterDto>, NotFound>> DeleteFilterAsync(Guid filterId,
        CancellationToken cancellationToken = default);

    Task<List<TableFilterDto>> GetFiltersAsync(EntityType tableType, CancellationToken cancellationToken = default);
}
