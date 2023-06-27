using TaxBeacon.Common.Services;

namespace TaxBeacon.API.Services;

public sealed class CurrentTimeZoneService: ICurrentTimeZoneService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string IanaTimeZoneParameterName = "IanaTimeZone";

    public CurrentTimeZoneService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public string IanaTimeZone
    {
        get
        {
            var timeZoneKeyValuePair = _httpContextAccessor
                ?.HttpContext
                ?.Request
                ?.Query
                ?.FirstOrDefault(x => x.Key.Equals(IanaTimeZoneParameterName, StringComparison.OrdinalIgnoreCase));
            return !string.IsNullOrEmpty(timeZoneKeyValuePair.GetValueOrDefault().Value.ToString()) ? timeZoneKeyValuePair.GetValueOrDefault().Value.ToString() : "GMT";
        }
    }
}