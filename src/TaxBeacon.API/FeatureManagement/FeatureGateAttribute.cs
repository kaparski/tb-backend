using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaxBeacon.API.Authentication;
using TaxBeacon.Common.FeatureManagement;

namespace TaxBeacon.API.FeatureManagement;

public sealed class FeatureGateAttribute: ActionFilterAttribute
{
    public string FeatureName { get; }

    public FeatureGateAttribute(string featureName) => FeatureName = featureName;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var featureFlagService = context.HttpContext.RequestServices.GetService<IFeatureFlagsService>();
        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();

        if (featureFlagService is not null && !featureFlagService.IsEnabled(FeatureName))
        {
            loggerFactory
                .CreateLogger<FeatureGateAttribute>()
                .LogWarning("User with id {userId} do not have access to feature {feature}",
                            context.HttpContext.User.FindFirst(Claims.UserId),
                            FeatureName);
            context.Result = new ForbidResult();
        }

        base.OnActionExecuting(context);
    }
}
