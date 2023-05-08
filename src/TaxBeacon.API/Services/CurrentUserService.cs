using TaxBeacon.API.Authentication;
using TaxBeacon.Common.Roles;
using TaxBeacon.Common.Services;

namespace TaxBeacon.API.Services;

public class CurrentUserService: ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Guid UserId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User.Claims.SingleOrDefault(c => c.Type == Claims.UserIdClaimName)?.Value, out var userId)
            ? userId
    : Guid.Empty;

    public bool IsSuperAdmin => _httpContextAccessor?.HttpContext
                                                    ?.User
                                                    ?.Claims
                                                    ?.Where(c => c.Type == Claims.Roles)
                                                    ?.Select(c => c.Value)
                                                    ?.ToHashSet()
                                                    ?.Contains(Roles.SuperAdmin) == true;

    public Guid TenantId =>
        IsSuperAdmin &&
        _httpContextAccessor?.HttpContext?.Request?.Headers?.TryGetValue(Headers.SuperAdminTenantId, out var superAdminTenantIdString) == true &&
        Guid.TryParse(superAdminTenantIdString, out var superAdminTenantId)
        ? superAdminTenantId
        : Guid.TryParse(_httpContextAccessor?.HttpContext?.User.Claims.SingleOrDefault(c => c.Type == Claims.TenantId)?.Value, out var tenantId)
            ? tenantId
            : Guid.Empty;

    public (string FullName, string Roles) UserInfo
    {
        get
        {
            var fullName = _httpContextAccessor?.HttpContext
                                                    ?.User
                                                    ?.Claims
                                                    ?.SingleOrDefault(c => c.Type == Claims.FullName)?.Value ?? throw new InvalidOperationException();
            var roles = _httpContextAccessor?.HttpContext
                                                    ?.User
                                                    ?.Claims
                                                    ?.Where(c => c.Type == Claims.TenantRoles)?.Select(c => c.Value) ?? Enumerable.Empty<string>();

            return (fullName, string.Join(", ", roles));
        }
    }
}
