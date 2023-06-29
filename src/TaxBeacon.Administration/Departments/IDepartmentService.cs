using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.Administration.Departments.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Departments;

public interface IDepartmentService
{
    IQueryable<DepartmentDto> QueryDepartments();

    Task<QueryablePaging<DepartmentDto>> GetDepartmentsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportDepartmentsAsync(FileType fileType,
        CancellationToken cancellationToken = default);

    Task<OneOf<ActivityDto, NotFound>> GetActivityHistoryAsync(Guid id, int page, int pageSize,
        CancellationToken cancellationToken = default);

    Task<OneOf<DepartmentDetailsDto, NotFound>> GetDepartmentDetailsAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<OneOf<DepartmentDetailsDto, NotFound, InvalidOperation>> UpdateDepartmentAsync(Guid id, UpdateDepartmentDto updatedEntity,
            CancellationToken cancellationToken = default);

    Task<OneOf<QueryablePaging<DepartmentUserDto>, NotFound>> GetDepartmentUsersAsync(Guid departmentId,
        GridifyQuery gridifyQuery, CancellationToken cancellationToken = default);

    Task<IQueryable<DepartmentUserDto>> QueryDepartmentUsersAsync(Guid departmentId);

    Task<OneOf<DepartmentServiceAreaDto[], NotFound>> GetDepartmentServiceAreasAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<OneOf<DepartmentJobTitleDto[], NotFound>> GetDepartmentJobTitlesAsync(Guid id,
        CancellationToken cancellationToken = default);
}
