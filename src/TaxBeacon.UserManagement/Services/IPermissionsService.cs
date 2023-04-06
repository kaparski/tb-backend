using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services
{
    public interface IPermissionsService
    {
        Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid tenantId, Guid userId);
        Task<IReadOnlyCollection<PermissionDto>> GetPermissionsByRoleIdAsync(Guid tenantId, Guid roleId, CancellationToken cancellationToken = default);
    }
}
