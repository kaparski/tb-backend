using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface IRoleService
{
    Task<QueryablePaging<RoleDto>> GetRolesAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<OneOf<QueryablePaging<UserDto>, NotFound>> GetRoleAssignedUsersAsync(Guid roleId, GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);
}
