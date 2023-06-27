using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Errors;
using TaxBeacon.UserManagement.TableFilters.Models;

namespace TaxBeacon.UserManagement.TableFilters;

public interface ITableFiltersService
{
    Task<OneOf<List<TableFilterDto>, NameAlreadyExists>> CreateFilterAsync(CreateTableFilterDto createTableFilterDto,
        CancellationToken cancellationToken = default);

    Task<OneOf<List<TableFilterDto>, NotFound>> DeleteFilterAsync(Guid filterId,
        CancellationToken cancellationToken = default);

    Task<List<TableFilterDto>> GetFiltersAsync(EntityType tableType, CancellationToken cancellationToken = default);
}
