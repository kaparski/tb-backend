using System.Security.Claims;
using TaxBeacon.API.Authentication;
using TaxBeacon.Common.Permissions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.API.Services;

public class CurrentUserService: ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITaxBeaconDbContext _context;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ITaxBeaconDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public Guid UserId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirstValue(Claims.UserIdClaimName), out var userId)
            ? userId
            : Guid.Empty;

    public Guid TenantId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirstValue(Claims.TenantId), out var tenantId)
            ? tenantId
            : Guid.Empty;

    public (string FullName, string Roles) UserInfo
    {
        get
        {
            var user = _context
                .Users
                .Where(u => u.Id == UserId)
                .Select(u => new
                {
                    u.FullName,
                    Roles = string.Join(", ", _context
                        .TenantUserRoles
                        .Where(r => r.UserId == UserId && r.TenantId == TenantId)
                        .Select(r => r.TenantRole.Role.Name)
                    )
                })
                .Single();

            return (user.FullName, user.Roles);
        }
    }
}
