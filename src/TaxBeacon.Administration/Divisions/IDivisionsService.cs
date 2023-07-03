using OneOf;
using OneOf.Types;
using TaxBeacon.Administration.Divisions.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Divisions;

public interface IDivisionsService
{
    IQueryable<DivisionDto> QueryDivisions();

    Task<byte[]> ExportDivisionsAsync(FileType fileType,
    CancellationToken cancellationToken);

    Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid divisionId, uint page = 1,
        uint pageSize = 10, CancellationToken cancellationToken = default);

    Task<OneOf<DivisionDetailsDto, NotFound>> GetDivisionDetailsAsync(Guid divisionId, CancellationToken cancellationToken = default);

    Task<OneOf<DivisionDetailsDto, NotFound, InvalidOperation>> UpdateDivisionAsync(Guid id, UpdateDivisionDto updateDivisionDto, CancellationToken cancellationToken = default);

    Task<OneOf<DivisionDepartmentDto[], NotFound>> GetDivisionDepartmentsAsync(Guid id,
            CancellationToken cancellationToken = default);

    Task<IQueryable<DivisionUserDto>> QueryDivisionUsersAsync(Guid divisionId);
}
