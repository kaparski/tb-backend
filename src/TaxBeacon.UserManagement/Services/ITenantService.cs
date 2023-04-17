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

    Task<OneOf<QueryablePaging<DepartmentDto>, NotFound>> GetDepartmentsAsync(Guid tenantId,
        GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportDepartmentsAsync(Guid tenantId,
        FileType fileType,
        CancellationToken cancellationToken);
}
