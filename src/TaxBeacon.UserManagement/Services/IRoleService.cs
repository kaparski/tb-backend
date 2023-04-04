using Gridify;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface IRoleService
{
    Task<QueryablePaging<RoleDto>> GetRolesAsync(Guid tenantId, GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<QueryablePaging<UserDto>> GetRoleAssignedUsersAsync(Guid tenantId, Guid roleId, GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);
}
