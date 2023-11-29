using TaxBeacon.Common.Extensions;
using TimeZoneNames;

namespace TaxBeacon.Common.Services;

public sealed class DateTimeFormatter: IDateTimeFormatter
{
    private readonly ICurrentTimeZoneService _currentTimeZoneService;
    private const string FormatWithTime = "MM.dd.yyyy tt";
    private const string FormatDateOnly = "MM.dd.yyyy";

    public DateTimeFormatter(ICurrentTimeZoneService currentTimeZoneService) => _currentTimeZoneService = currentTimeZoneService;

    public string FormatDateTime(DateTime date)
    {
        ConvertToTimezone(date, out var tz, out var abbreviations, out var value);

        return tz.IsDaylightSavingTime(value)
            ? $"{value.ToString(FormatWithTime)} {abbreviations.Daylight}"
            : $"{value.ToString(FormatWithTime)} {abbreviations.Standard}";
    }

    public string FormatDateTime(DateTime? date) => date.HasValue ? FormatDateTime(date.Value) : string.Empty;

    public string FormatDate(DateTime date)
    {
        ConvertToTimezone(date, out var _, out var _, out var value);

        return $"{value.ToString(FormatDateOnly)}";
    }

    public string FormatDate(DateTime? date) => date.HasValue ? FormatDate(date.Value) : string.Empty;

    private void ConvertToTimezone(DateTime date, out TimeZoneInfo tz, out TimeZoneValues abbreviations, out DateTime value)
    {
        TimeZoneInfo.TryConvertIanaIdToWindowsId(_currentTimeZoneService.IanaTimeZone, out var windowsId);

        windowsId ??= "UTC";

        tz = TimeZoneInfo.FindSystemTimeZoneById(windowsId);
        abbreviations = TZNames.GetAbbreviationsForTimeZone(windowsId, "en-US");
        value = date.ConvertUtcDateToTimeZone(_currentTimeZoneService.IanaTimeZone);
    }
}
