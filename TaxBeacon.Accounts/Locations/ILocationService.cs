using OneOf;
using OneOf.Types;
using TaxBeacon.Accounts.Locations.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Locations;

public interface ILocationService
{
    OneOf<IQueryable<LocationDto>, NotFound> QueryLocations(Guid accountId);

    Task<OneOf<LocationDetailsDto, NotFound>> GetLocationDetailsAsync(Guid accountId,
        Guid locationId,
        CancellationToken cancellationToken = default);

    Task<OneOf<LocationDetailsDto, NotFound>> CreateNewLocationAsync(Guid accountId,
        CreateLocationDto createLocationDto,
        CancellationToken cancellationToken = default);

    Task<OneOf<LocationDetailsDto, NotFound>> UpdateLocationStatusAsync(Guid accountId, Guid locationId,
        Status status, CancellationToken cancellationToken = default);

    Task<OneOf<LocationDetailsDto, NotFound>> UpdateLocationAsync(Guid accountId,
        Guid locationId,
        UpdateLocationDto updateLocation,
        CancellationToken cancellationToken = default);

    Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid accountId,
        Guid locationId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<OneOf<byte[], NotFound>> ExportLocationsAsync(Guid accountId,
        FileType fileType,
        CancellationToken cancellationToken = default);

    Task<OneOf<string, InvalidOperation>> GenerateUniqueLocationIdAsync(CancellationToken cancellationToken);
}
