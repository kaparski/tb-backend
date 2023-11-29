using Microsoft.Extensions.Options;
using System.Text.Json;
using TaxBeacon.API.Extensions;
using TaxBeacon.Common.FeatureManagement;

namespace TaxBeacon.API.FeatureManagement;

public class FeatureFlagsService: IFeatureFlagsService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly FeatureFlags _featureFlags;
    private IDictionary<string, bool>? _currentFeatureFlags;

    public FeatureFlagsService(IHttpContextAccessor httpContextAccessor, FeatureFlags featureFlags)
    {
        _httpContextAccessor = httpContextAccessor;
        _featureFlags = featureFlags;
    }

    public IDictionary<string, bool> GetCurrentFeatureFlags()
    {
        if (_currentFeatureFlags is not null)
        {
            return _currentFeatureFlags;
        }

        var featureFlagsCookie =
            _httpContextAccessor?.HttpContext?.Request?.Cookies?.GetChunkedValue(nameof(FeatureFlags));

        if (!string.IsNullOrEmpty(featureFlagsCookie))
        {
            var featureFlagsFromCookie = JsonSerializer.Deserialize<IDictionary<string, bool>>(featureFlagsCookie,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, });

            if (featureFlagsFromCookie is not null)
            {
                _currentFeatureFlags = featureFlagsFromCookie;
            }
        }

        return _currentFeatureFlags ??= _featureFlags.Flags;
    }

    public IDictionary<string, bool> UpdateFeatureFlag(IDictionary<string, bool> newFeatureFlags)
    {
        var currentFeatureFlags = GetCurrentFeatureFlags();
        foreach (var (key, value) in newFeatureFlags)
        {
            if (currentFeatureFlags.ContainsKey(key))
            {
                currentFeatureFlags[key] = value;
            }
        }

        UpdateFeatureFlagsInCookies(currentFeatureFlags);

        return currentFeatureFlags;
    }

    public bool IsEnabled(string key) => GetCurrentFeatureFlags().TryGetValue(key, out var value) && value;

    public IDictionary<string, bool> ResetFeatureFlags()
    {
        _currentFeatureFlags = _featureFlags.Flags;
        UpdateFeatureFlagsInCookies(_currentFeatureFlags);

        return _currentFeatureFlags;
    }

    private void UpdateFeatureFlagsInCookies(IDictionary<string, bool> featureFlags)
    {
        var options = new CookieOptions
        {
            Secure = true,
            HttpOnly = false,
            Expires = DateTime.UtcNow.AddDays(30),
            SameSite = SameSiteMode.Lax,
            Domain = _httpContextAccessor?.HttpContext?.GetDomainName()
        };

        _httpContextAccessor?.HttpContext?.Response?.Cookies?.WriteChunkedValue(
            nameof(FeatureFlags),
            JsonSerializer.Serialize(featureFlags,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, }),
            options);
    }
}
