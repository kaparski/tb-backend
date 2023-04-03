using Gridify;
using OneOf;
using OneOf.Types;
using System.Collections;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface IRoleService
{
    Task<QueryablePaging<RoleDto>> GetRolesAsync(Guid tenantId, GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<OneOf<Success, NotFound>> UnassignUsersAsync(List<Guid> users, Guid tenantId, Guid roleId, Guid currentUserId, CancellationToken cancellationToken);
}
