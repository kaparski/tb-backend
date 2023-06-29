namespace TaxBeacon.Common.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ConvertUtcDateToTimeZone(this DateTime utcDateTime, string ianaTimeZone)
    {

        if (string.IsNullOrEmpty(ianaTimeZone))
        {
            return utcDateTime;
        }

        var windowsTimeZoneName = TimeZoneConverter.TZConvert.IanaToWindows(ianaTimeZone);
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneName);

        if (timeZoneInfo == TimeZoneInfo.Utc)
        {
            return utcDateTime;
        }

        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
    }

    public static DateTime? ConvertUtcDateToTimeZone(this DateTime? utcDateTime, string ianaTimeZone)
        => utcDateTime.HasValue ? utcDateTime.Value.ConvertUtcDateToTimeZone(ianaTimeZone) : utcDateTime;
}