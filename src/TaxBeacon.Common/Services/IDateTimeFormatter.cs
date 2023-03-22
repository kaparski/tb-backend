namespace TaxBeacon.Common.Services
{
    public interface IDateTimeFormatter
    {
        string FormatDate(DateTime date);

        string FormatDate(DateTime? date);

    }
}
