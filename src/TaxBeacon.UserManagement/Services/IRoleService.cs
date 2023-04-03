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

    Task<OneOf<Success, NotFound>> UnassignUsers(List<Guid> users, Guid tenantId, CancellationToken cancellationToken, Guid roleId);
}
