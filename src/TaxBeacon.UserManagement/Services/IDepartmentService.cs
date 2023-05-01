using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface IDepartmentService
{
    Task<OneOf<QueryablePaging<DepartmentDto>, NotFound>> GetDepartmentsAsync(Guid tenantId,
        GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportDepartmentsAsync(Guid tenantId,
        FileType fileType,
        CancellationToken cancellationToken);

    Task<OneOf<ActivityDto, NotFound>> GetActivityHistoryAsync(Guid id, int page, int pageSize, CancellationToken cancellationToken);

    Task<OneOf<DepartmentDto, NotFound>> GetDepartmentDetailsAsync(Guid id, CancellationToken cancellationToken);

    Task<OneOf<DepartmentDto, NotFound>> UpdateDepartmentAsync(Guid id, UpdateDepartmentDto updatedEntity,
            CancellationToken cancellationToken = default);
}
