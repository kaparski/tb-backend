using System.Security.Claims;
using TaxBeacon.API.Authentication;
using TaxBeacon.Common.Services;

namespace TaxBeacon.API.Services;

public class CurrentUserService: ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Guid UserId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User.FindFirstValue(Claims.UserIdClaimName), null, out var id)
            ? id
            : Guid.Empty;

    public Guid TenantId =>
        Guid.TryParse(_httpContextAccessor.HttpContext!.User!.FindFirstValue(Claims.TenantId), null, out var id)
            ? id
            : Guid.Empty;
}
