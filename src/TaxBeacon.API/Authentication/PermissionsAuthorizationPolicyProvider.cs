using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace TaxBeacon.API.Authentication
{
    public sealed class PermissionsAuthorizationPolicyProvider
        : DefaultAuthorizationPolicyProvider
    {
        public PermissionsAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
            : base(options)
        {
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            var policy = await base.GetPolicyAsync(policyName);

            return policy ?? new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionsRequirement(policyName))
                .Build();

        }
    }
}
