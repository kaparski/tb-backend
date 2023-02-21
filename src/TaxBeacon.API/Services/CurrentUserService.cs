using System.Security.Claims;
using TaxBeacon.Common.Services;

namespace TaxBeacon.API.Services;

public class CurrentUserService: ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue("userId") ?? string.Empty;
}
