using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public class RoleService: IRoleService
{
    private readonly ITaxBeaconDbContext _context;
    public RoleService(ITaxBeaconDbContext context) => _context = context;

    public async Task<QueryablePaging<RoleDto>> GetRolesAsync(Guid tenantId, GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
    {
        tenantId = tenantId != default ? tenantId : (await _context.Tenants.FirstAsync(cancellationToken)).Id;

        return await _context.TenantRoles
            .Where(tr => tr.TenantId == tenantId)
            .Select(tr => new RoleDto
            {
                Id = tr.RoleId,
                Name = tr.Role.Name,
                AssignedUsersCount = tr.TenantUserRoles.Count
            })
            .AsNoTracking()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);
    }
}
