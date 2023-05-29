using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface IDepartmentService
{
    Task<QueryablePaging<DepartmentDto>> GetDepartmentsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportDepartmentsAsync(FileType fileType,
        CancellationToken cancellationToken = default);

    Task<OneOf<ActivityDto, NotFound>> GetActivityHistoryAsync(Guid id, int page, int pageSize,
        CancellationToken cancellationToken = default);

    Task<OneOf<DepartmentDetailsDto, NotFound>> GetDepartmentDetailsAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<OneOf<DepartmentDetailsDto, NotFound>> UpdateDepartmentAsync(Guid id, UpdateDepartmentDto updatedEntity,
            CancellationToken cancellationToken = default);

    Task<OneOf<QueryablePaging<DepartmentUserDto>, NotFound>> GetDepartmentUsersAsync(Guid departmentId,
        GridifyQuery gridifyQuery, CancellationToken cancellationToken = default);

    Task<OneOf<DepartmentServiceAreaDto[], NotFound>> GetDepartmentServiceAreasAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<OneOf<DepartmentJobTitleDto[], NotFound>> GetDepartmentJobTitlesAsync(Guid id,
        CancellationToken cancellationToken = default);
}
