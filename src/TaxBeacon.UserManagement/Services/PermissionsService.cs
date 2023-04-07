using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.UserManagement.Services
{
    public class PermissionsService: IPermissionsService
    {
        private readonly ILogger<PermissionsService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITaxBeaconDbContext _context;

        public PermissionsService(ITaxBeaconDbContext context,
            ILogger<PermissionsService> logger,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid userId) =>
            _currentUserService.TenantId == default
                ? await GetPermissionsByUserIdAsync(userId)
                : await GetTenantPermissionsByUserIdAsync(_currentUserService.TenantId, userId);

        private async Task<IReadOnlyCollection<string>> GetTenantPermissionsByUserIdAsync(Guid tenantId, Guid userId) =>
            await _context.TenantUserRoles
                .Where(tur => tur.TenantId == tenantId && tur.UserId == userId)
                .Join(_context.TenantRolePermissions,
                    tur => new { tur.TenantId, tur.RoleId },
                    trp => new { trp.TenantId, trp.RoleId },
                    (tur, trp) => trp.PermissionId)
                .Join(_context.Permissions, id => id, p => p.Id, (id, p) => p.Name)
                .AsNoTracking()
                .ToListAsync();

        private async Task<IReadOnlyCollection<string>> GetPermissionsByUserIdAsync(Guid userId) =>
            await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId)
                .Join(_context.RolePermissions,
                    ur => new { ur.RoleId },
                    rp => new { rp.RoleId },
                    (ur, rp) => rp.PermissionId)
                .Join(_context.Permissions, id => id, p => p.Id, (id, p) => p.Name)
                .ToListAsync();
    }
}
