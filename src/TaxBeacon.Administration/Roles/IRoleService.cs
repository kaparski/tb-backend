using OneOf;
using OneOf.Types;
using TaxBeacon.Administration.Roles.Models;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.Roles;

public interface IRoleService
{
    IQueryable<RoleDto> QueryRoles();

    Task<IQueryable<RoleAssignedUserDto>> QueryRoleAssignedUsersAsync(Guid roleId);

    Task<OneOf<Success, NotFound>> UnassignUsersAsync(Guid roleId,
        List<Guid> users,
        CancellationToken cancellationToken = default);

    Task<OneOf<Success, NotFound>> AssignUsersAsync(Guid roleId,
        List<Guid> userIds,
        CancellationToken cancellationToken = default);

    Task<OneOf<IReadOnlyCollection<PermissionDto>, NotFound>> GetRolePermissionsByIdAsync(Guid roleId,
        CancellationToken cancellationToken = default);

    Task<OneOf<RoleDto, NotFound>> GetRoleByIdAsync(Guid roleId,
        CancellationToken cancellationToken = default);
}
