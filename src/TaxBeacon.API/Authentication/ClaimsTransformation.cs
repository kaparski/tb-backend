using Microsoft.AspNetCore.Authentication;
using System.Net.Mail;
using System.Security.Claims;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Authentication
{
    public sealed class ClaimsTransformation: IClaimsTransformation
    {
        private readonly IUserService _userService;
        public ClaimsTransformation(IUserService userService) => _userService = userService;

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var email = principal.Claims
                        .FirstOrDefault(claim => claim.Type.Equals(Claims.EmailClaimName, StringComparison.OrdinalIgnoreCase))
                        ?.Value;

            if (email == null)
            {
                return principal;
            }

            var claimsIdentity = new ClaimsIdentity();

            if (!principal.HasClaim(claim => claim.Type == Claims.UserIdClaimName))
            {
                var user = await _userService.GetUserByEmailAsync(new MailAddress(email), default);

                var userId = user?.Id.ToString() ?? string.Empty;
                claimsIdentity.AddClaim(new Claim(Claims.UserIdClaimName, userId));

                if (!principal.HasClaim(claim => claim.Type == Claims.TenantId))
                {
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var tenantId = await _userService.GetTenantIdAsync(user!.Id);
                        claimsIdentity.AddClaim(new Claim(Claims.TenantId, tenantId.ToString()));
                    }
                }
            }

            principal.AddIdentity(claimsIdentity);
            return principal;
        }
    }
}
