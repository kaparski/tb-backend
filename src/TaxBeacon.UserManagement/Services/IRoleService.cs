using Gridify;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface IRoleService
{
    Task<QueryablePaging<RoleDto>> GetRolesAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);
}
