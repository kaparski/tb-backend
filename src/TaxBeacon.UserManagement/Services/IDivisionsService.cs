using Gridify;
using OneOf.Types;
using OneOf;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Errors;

namespace TaxBeacon.UserManagement.Services;

public interface IDivisionsService
{
    IQueryable<DivisionDto> QueryDivisions();

    Task<QueryablePaging<DivisionDto>> GetDivisionsAsync(GridifyQuery gridifyQuery,
    CancellationToken cancellationToken = default);

    Task<byte[]> ExportDivisionsAsync(FileType fileType,
    CancellationToken cancellationToken);

    Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid divisionId, uint page = 1,
        uint pageSize = 10, CancellationToken cancellationToken = default);

    Task<OneOf<DivisionDetailsDto, NotFound>> GetDivisionDetailsAsync(Guid divisionId, CancellationToken cancellationToken = default);

    Task<OneOf<QueryablePaging<DivisionUserDto>, NotFound>> GetDivisionUsersAsync(Guid divisionId, GridifyQuery gridifyQuery, CancellationToken cancellationToken = default);

    Task<OneOf<DivisionDetailsDto, NotFound, InvalidOperation>> UpdateDivisionAsync(Guid id, UpdateDivisionDto updateDivisionDto, CancellationToken cancellationToken = default);

    Task<OneOf<DivisionDepartmentDto[], NotFound>> GetDivisionDepartmentsAsync(Guid id,
            CancellationToken cancellationToken = default);

    Task<IQueryable<DivisionUserDto>> QueryDivisionUsersAsync(Guid divisionId);
}
