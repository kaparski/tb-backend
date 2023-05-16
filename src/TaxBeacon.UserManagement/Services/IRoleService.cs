using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface IRoleService
{
    Task<QueryablePaging<RoleDto>> GetRolesAsync(IGridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<OneOf<QueryablePaging<RoleAssignedUserDto>, NotFound>> GetRoleAssignedUsersAsync(Guid roleId,
        IGridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<OneOf<Success, NotFound>> UnassignUsersAsync(Guid roleId,
        List<Guid> users,
        CancellationToken cancellationToken);

    Task<OneOf<Success, NotFound>> AssignUsersAsync(Guid roleId,
        List<Guid> userIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PermissionDto>> GetRolePermissionsByIdAsync(Guid roleId,
        CancellationToken cancellationToken = default);
}
