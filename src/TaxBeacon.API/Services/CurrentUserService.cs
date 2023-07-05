using System.Security.Claims;
using TaxBeacon.API.Authentication;
using TaxBeacon.Common.Services;
using RolesConstants = TaxBeacon.Common.Constants.Roles;

namespace TaxBeacon.API.Services;

public class CurrentUserService: ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Guid UserId =>
        Guid.TryParse(UserClaims?.SingleOrDefault(c => c.Type == Claims.UserIdClaimName)?.Value, out var userId)
            ? userId
            : Guid.Empty;

    public bool IsSuperAdmin => UserClaims
                                    ?.Where(c => c.Type == Claims.Roles)
                                    ?.Select(c => c.Value)
                                    ?.ToHashSet()
                                    ?.Contains(RolesConstants.SuperAdmin)
                                == true;

    public bool IsUserInTenant => TenantId != default;

    public Guid TenantId =>
        IsSuperAdmin
        && _httpContextAccessor?.HttpContext?.Request?.Headers?
            .TryGetValue(Headers.SuperAdminTenantId, out var superAdminTenantIdString)
        == true
        && Guid.TryParse(superAdminTenantIdString, out var superAdminTenantId)
            ? superAdminTenantId
            : Guid.TryParse(UserClaims?.SingleOrDefault(c => c.Type == Claims.TenantId)?.Value,
                out var tenantId)
                ? tenantId
                : Guid.Empty;

    public (string FullName, string Roles) UserInfo
    {
        get
        {
            var fullName = UserClaims?.SingleOrDefault(c => c.Type == Claims.FullName)?.Value
                           ?? throw new InvalidOperationException();

            return (fullName, string.Join(", ", TenantRoles.Concat(Roles).Order()));
        }
    }

    public IReadOnlyCollection<string> TenantRoles => (_httpContextAccessor?.HttpContext
                                                           ?.User
                                                           ?.Claims
                                                           ?.Where(c => c.Type == Claims.TenantRoles)
                                                           ?.Select(c => c.Value)
                                                       ?? Enumerable.Empty<string>()).ToArray();

    public IReadOnlyCollection<string> Roles => (_httpContextAccessor?.HttpContext
                                                     ?.User
                                                     ?.Claims
                                                     ?.Where(c => c.Type == Claims.Roles)?
                                                     .Select(c => c.Value)
                                                 ?? Enumerable.Empty<string>()).ToArray();

    public bool DivisionEnabled => bool.TryParse(UserClaims?.SingleOrDefault(c => c.Type == Claims.DivisionEnabled)?.Value, out var divisionEnabled)
        && divisionEnabled;

    private IEnumerable<Claim>? UserClaims => _httpContextAccessor?.HttpContext?.User?.Claims;
}
