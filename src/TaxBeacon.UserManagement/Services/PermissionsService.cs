using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services
{
    public class PermissionsService: IPermissionsService
    {
        private readonly ILogger<PermissionsService> _logger;
        private readonly ITaxBeaconDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public PermissionsService(ITaxBeaconDbContext context,
            ILogger<PermissionsService> logger,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<IReadOnlyCollection<PermissionDto>> GetPermissionsByRoleIdAsync(
            Guid roleId,
            CancellationToken cancellationToken = default)
        {
            var permissions = await _context.TenantRolePermissions
                .AsNoTracking()
                .Where(trp => trp.TenantId == _currentUserService.TenantId && trp.RoleId == roleId)
                .Join(_context.Permissions, trp => trp.PermissionId, p => p.Id, (trp, p) => new { p.Id, p.Name })
                .ProjectToType<PermissionDto>()
                .ToListAsync(cancellationToken);

            var permissionsWithCategory = permissions.Select(p => p with { Category = p.Name.Split('.')[0] }).ToList();
            return permissionsWithCategory;
        }
    }
}
