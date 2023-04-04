using Gridify;
using Gridify.EntityFramework;
using Mapster;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public class RoleService: IRoleService
{
    private readonly ITaxBeaconDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RoleService(ITaxBeaconDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public Task<QueryablePaging<RoleDto>> GetRolesAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) =>
        _context.TenantRoles
            .Where(tr => tr.TenantId == _currentUserService.TenantId)
            .Select(tr => new RoleDto
            {
                Id = tr.RoleId,
                Name = tr.Role.Name,
                AssignedUsersCount = tr.TenantUserRoles.Count
            })
            .AsNoTracking()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

    public async Task<OneOf<QueryablePaging<UserDto>, NotFound>> GetRoleAssignedUsersAsync(Guid roleId, GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
    {
        var users = await _context.TenantUserRoles
            .Where(tr => tr.TenantId == _currentUserService.TenantId && tr.RoleId == roleId)
            .Select(tr => tr.TenantUser.User)
            .ProjectToType<UserDto>()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

        return users.Count == 0 || gridifyQuery.Page != 1 && gridifyQuery.Page > Math.Ceiling((double)users.Count / gridifyQuery.PageSize)
            ? new NotFound()
            : users;
    }
}
