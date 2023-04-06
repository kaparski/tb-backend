using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.OpenXmlFormats.Wordprocessing;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;

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

        public async Task<IReadOnlyCollection<PermissionDto>> GetPermissionsByRoleIdAsync(
            Guid tenantId,
            Guid roleId,
            CancellationToken cancellationToken = default)
        {
            tenantId = tenantId != default ? tenantId : (await _context.Tenants.FirstAsync(cancellationToken)).Id;
            var permissions = await _context.TenantRolePermissions
                .Where(trp => trp.TenantId == tenantId && trp.RoleId == roleId)
                .Join(_context.Permissions, trp => trp.PermissionId, p => p.Id, (trp, p) => new { p.Id, p.Name })
                .ProjectToType<PermissionDto>()
                .AsNoTracking()
                .ToListAsync();

            permissions.ForEach(p => p.Category = p.Name.Split('.')[0]);
            return permissions;
        }
    }
}
