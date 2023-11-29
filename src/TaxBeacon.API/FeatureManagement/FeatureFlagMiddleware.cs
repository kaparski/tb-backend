using System.Text.Json;
using TaxBeacon.API.Extensions;
using TaxBeacon.Common.FeatureManagement;

namespace TaxBeacon.API.FeatureManagement;

public class FeatureFlagMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly FeatureFlags _featureFlags;

    public FeatureFlagMiddleware(RequestDelegate next,
        FeatureFlags featureFlags,
        IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
        _featureFlags = featureFlags;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (!_environment.IsDevelopment() && !_environment.IsCloudDevelopment())
        {
            SetupFlagsInCookies(httpContext, _featureFlags.Flags);
        }
        else
        {
            var cookie = httpContext.Request.Cookies.GetChunkedValue(nameof(FeatureFlags));
            var newFeatureFlags = new Dictionary<string, bool>(_featureFlags.Flags);

            if (!string.IsNullOrEmpty(cookie))
            {
                var flagsFromCookie = JsonSerializer.Deserialize<IDictionary<string, bool>>(cookie,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                if (flagsFromCookie is not null)
                {
                    foreach (var kvp in flagsFromCookie)
                    {
                        if (newFeatureFlags.ContainsKey(kvp.Key))
                        {
                            newFeatureFlags[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }

            SetupFlagsInCookies(httpContext, newFeatureFlags);
        }

        await _next(httpContext);
    }

    private void SetupFlagsInCookies(HttpContext httpContext, IDictionary<string, bool> featureFlags)
    {
        var options = new CookieOptions
        {
            Secure = true,
            HttpOnly = false,
            Expires = DateTime.UtcNow.AddDays(30),
            SameSite = SameSiteMode.Lax,
            Domain = httpContext.GetDomainName()
        };

        httpContext.Response.Cookies
            .WriteChunkedValue(
                nameof(FeatureFlags),
                JsonSerializer.Serialize(featureFlags,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                options);
    }
}
