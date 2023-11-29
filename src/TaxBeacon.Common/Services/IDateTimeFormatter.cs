namespace TaxBeacon.Common.Services;

public interface IDateTimeFormatter
{
    string FormatDateTime(DateTime date);

    string FormatDateTime(DateTime? date);

    string FormatDate(DateTime date);

    string FormatDate(DateTime? date);

}
