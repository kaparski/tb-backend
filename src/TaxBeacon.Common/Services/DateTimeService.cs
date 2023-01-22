namespace TaxBeacon.Common.Services;

public class DateTimeService: IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
