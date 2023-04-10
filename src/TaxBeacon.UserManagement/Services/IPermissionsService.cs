using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services
{
    public interface IPermissionsService
    {
        Task<IReadOnlyCollection<PermissionDto>> GetPermissionsByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    }
}
