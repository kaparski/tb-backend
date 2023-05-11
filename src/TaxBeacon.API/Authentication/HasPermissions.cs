using Microsoft.AspNetCore.Authorization;

namespace TaxBeacon.API.Authentication
{
    public class HasPermissions: AuthorizeAttribute
    {
        public HasPermissions(object permission) : base(policy: $"{permission.GetType().Name}.{permission}")
        {
        }

        public HasPermissions(params object[] permissions) => Policy = string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}"));
    }
}
