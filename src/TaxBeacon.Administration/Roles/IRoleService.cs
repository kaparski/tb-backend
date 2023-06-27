using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.Administration.Roles.Models;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.Administration.Roles;

public interface IRoleService
{
    IQueryable<RoleDto> QueryRoles();

    Task<IQueryable<RoleAssignedUserDto>> QueryRoleAssignedUsersAsync(Guid roleId);

    Task<QueryablePaging<RoleDto>> GetRolesAsync(IGridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<OneOf<QueryablePaging<RoleAssignedUserDto>, NotFound>> GetRoleAssignedUsersAsync(Guid roleId,
        IGridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<OneOf<Success, NotFound>> UnassignUsersAsync(Guid roleId,
        List<Guid> users,
        CancellationToken cancellationToken = default);

    Task<OneOf<Success, NotFound>> AssignUsersAsync(Guid roleId,
        List<Guid> userIds,
        CancellationToken cancellationToken = default);

    Task<OneOf<IReadOnlyCollection<PermissionDto>, NotFound>> GetRolePermissionsByIdAsync(Guid roleId,
        CancellationToken cancellationToken = default);

    Task<OneOf<Role, NotFound>> GetRoleByIdAsync(Guid roleId,
        CancellationToken cancellationToken = default);
}
