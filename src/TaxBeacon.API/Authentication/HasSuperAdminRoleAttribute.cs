using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaxBeacon.Common.Services;

namespace TaxBeacon.API.Authentication;

public class HasSuperAdminRoleAttribute: ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        var service = context.HttpContext.RequestServices.GetService<ICurrentUserService>();
        if (service is not null && !service.IsSuperAdmin)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status404NotFound);
        }
    }
}
