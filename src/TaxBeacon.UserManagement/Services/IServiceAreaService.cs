using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface IServiceAreaService
{
    Task<QueryablePaging<ServiceAreaDto>> GetServiceAreasAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken);

    Task<byte[]> ExportServiceAreasAsync(FileType fileType,
        CancellationToken cancellationToken);

    Task<OneOf<ServiceAreaDetailsDto, NotFound>> GetServiceAreaDetailsByIdAsync(Guid id,
        CancellationToken cancellationToken);

    Task<OneOf<ServiceAreaDetailsDto, NotFound>> UpdateServiceAreaDetailsAsync(Guid id,
        UpdateServiceAreaDto updateServiceAreaDto,
        CancellationToken cancellationToken);

    Task<OneOf<ActivityDto, NotFound>> GetActivityHistoryAsync(Guid id,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<OneOf<QueryablePaging<ServiceAreaUserDto>, NotFound>> GetUsersAsync(Guid serviceAreaId,
        GridifyQuery gridifyQuery,
        CancellationToken cancellationToken);
}
