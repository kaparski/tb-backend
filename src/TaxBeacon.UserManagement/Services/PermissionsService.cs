using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.UserManagement.Services
{
    public class PermissionsService: IPermissionsService
    {
        private readonly ILogger<PermissionsService> _logger;
        private readonly ITaxBeaconDbContext _context;

        public PermissionsService(ITaxBeaconDbContext context, ILogger<PermissionsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid tenantId, Guid userId) =>
            await _context.TenantUserRoles
                .Where(tur => tur.TenantId == tenantId && tur.UserId == userId)
                .Join(_context.TenantRolePermissions, tur => new { tur.TenantId, tur.RoleId }, trp => new { trp.TenantId, trp.RoleId }, (tur, trp) => trp.PermissionId)
                .Join(_context.Permissions, id => id, p => p.Id, (id, p) => p.Name)
                .AsNoTracking()
                .ToListAsync();
    }
}
