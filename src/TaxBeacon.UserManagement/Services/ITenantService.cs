using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface ITenantService
{
    Task<OneOf<QueryablePaging<TenantDto>, NotFound>> GetTenantsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportTenantsAsync(FileType fileType, CancellationToken cancellationToken);

    Task<OneOf<TenantDto, NotFound>> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken);
}
