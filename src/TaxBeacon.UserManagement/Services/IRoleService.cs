using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface IRoleService
{
    Task<QueryablePaging<RoleDto>> GetRolesAsync(Guid tenantId, GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<OneOf<Success, NotFound>> UnassignUsersAsync(Guid roleId, List<Guid> users, CancellationToken cancellationToken);
}
