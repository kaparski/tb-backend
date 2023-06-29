using Mapster;
using OneOf;
using OneOf.Types;
using TaxBeacon.Accounts.Locations.Models;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;

namespace TaxBeacon.Accounts.Locations;

public class LocationService: ILocationService
{
    private readonly IAccountDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public LocationService(IAccountDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public OneOf<IQueryable<LocationDto>, NotFound> QueryLocations(Guid accountId)
    {
        var tenantId = _currentUserService.TenantId;

        var accountExists = _context.Accounts.Any(acc => acc.Id == accountId && acc.TenantId == tenantId);

        if (!accountExists)
        {
            return new NotFound();
        }

        var locations = _context.Locations
            .Where(l => l.AccountId == accountId)
            .ProjectToType<LocationDto>();

        return OneOf<IQueryable<LocationDto>, NotFound>.FromT0(locations);
    }
}
