using TaxBeacon.Common.Extensions;
using TimeZoneNames;

namespace TaxBeacon.Common.Services
{
    public sealed class DateTimeFormatter: IDateTimeFormatter
    {
        private readonly ICurrentTimeZoneService _currentTimeZoneService;
        private const string Format = "MM.dd.yyyy hh:mm:ss tt";

        public DateTimeFormatter(ICurrentTimeZoneService currentTimeZoneService) => _currentTimeZoneService = currentTimeZoneService;

        public string FormatDate(DateTime date)
        {
            TimeZoneInfo.TryConvertIanaIdToWindowsId(_currentTimeZoneService.IanaTimeZone, out var windowsId);

            windowsId ??= "UTC";

            var tz = TimeZoneInfo.FindSystemTimeZoneById(windowsId);
            var abbreviations = TZNames.GetAbbreviationsForTimeZone(windowsId, "en-US");
            var value = date.ConvertUtcDateToTimeZone(_currentTimeZoneService.IanaTimeZone);

            return tz.IsDaylightSavingTime(value)
                    ? $"{value.ToString(Format)} {abbreviations.Daylight}"
                    : $"{value.ToString(Format)} {abbreviations.Standard}";
        }

        public string FormatDate(DateTime? date) => date.HasValue ? FormatDate(date.Value) : string.Empty;
    }
}
