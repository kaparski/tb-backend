using Gridify;
using Gridify.EntityFramework;
using Mapster;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public class RoleService: IRoleService
{
    private readonly ITaxBeaconDbContext _context;
    public RoleService(ITaxBeaconDbContext context) => _context = context;

    public async Task<QueryablePaging<RoleDto>> GetRolesAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) =>
        await _context.Roles
            .ProjectToType<RoleDto>()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);
}
