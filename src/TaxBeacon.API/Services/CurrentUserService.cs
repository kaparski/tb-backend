using System.Security.Claims;
using TaxBeacon.API.Authentication;
using TaxBeacon.Common.Roles;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.API.Services;

public class CurrentUserService: ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITaxBeaconDbContext _dbContext;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ITaxBeaconDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public Guid UserId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirstValue(Claims.UserIdClaimName), out var userId)
            ? userId
    : Guid.Empty;

    public bool IsSuperAdmin => _dbContext?.UserRoles?.Any(ur => ur.UserId == UserId && ur.Role.Name == Roles.SuperAdmin) == true;

    public Guid TenantId =>
        IsSuperAdmin &&
        _httpContextAccessor?.HttpContext?.Request?.Headers?.TryGetValue(Headers.SuperAdminTenantId, out var superAdminTenantIdString) == true &&
        Guid.TryParse(superAdminTenantIdString, out var superAdminTenantId)
        ? superAdminTenantId
        : Guid.TryParse(_httpContextAccessor?.HttpContext?.User.FindFirstValue(Claims.TenantId), out var tenantId)
            ? tenantId
            : Guid.Empty;

}
