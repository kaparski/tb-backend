using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Erros;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface ITableFiltersService
{
    Task<OneOf<TableFilterDto, NameAlreadyExists>> CreateFilterAsync(CreateTableFilterDto createTableFilterDto,
        CancellationToken cancellationToken = default);

    Task<OneOf<Success, NotFound>> DeleteFilterAsync(Guid filterId,
        CancellationToken cancellationToken = default);

    Task<List<TableFilterDto>> GetFiltersAsync(EntityType tableType, CancellationToken cancellationToken = default);
}
