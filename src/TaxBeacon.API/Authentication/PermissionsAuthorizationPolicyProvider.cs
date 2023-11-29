using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace TaxBeacon.API.Authentication;

public sealed class PermissionsAuthorizationPolicyProvider
    : DefaultAuthorizationPolicyProvider
{
    private readonly LoadTestingOptions _loadTestingOptions;

    public PermissionsAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options, IOptions<LoadTestingOptions> loadTestingOptions)
        : base(options) => _loadTestingOptions = loadTestingOptions.Value;

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);

        return policy ?? (_loadTestingOptions.IsLoadTestingEnabled == true
                            ? new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme, Schemas.LoadTestingSchema)
                            : new AuthorizationPolicyBuilder())
            .AddRequirements(new PermissionsRequirement(policyName))
            .Build();

    }
}
