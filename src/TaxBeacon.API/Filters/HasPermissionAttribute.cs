using Microsoft.AspNetCore.Authorization;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.API.Filters;

public sealed class HasPermissionAttribute: AuthorizeAttribute
{
    public HasPermissionAttribute(PermissionEnum permissionEnum)
        : base(policy: permissionEnum.ToString())
    {

    }
}
