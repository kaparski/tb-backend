using OneOf;
using OneOf.Types;
using TaxBeacon.Administration.ServiceAreas.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.ServiceAreas;

public interface IServiceAreaService
{
    IQueryable<ServiceAreaDto> QueryServiceAreas();

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

    Task<IQueryable<ServiceAreaUserDto>> QueryUsersAsync(Guid serviceAreaId);
}
