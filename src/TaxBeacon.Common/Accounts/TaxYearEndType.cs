using Ardalis.SmartEnum;

namespace TaxBeacon.Common.Accounts;

public class TaxYearEndType: SmartEnum<TaxYearEndType>
{
    public static readonly TaxYearEndType None = new("None", 0);
    public static readonly TaxYearEndType Calendar = new("Calendar year", 1);
    public static readonly TaxYearEndType FiscalStandard = new("Fiscal standard", 2);
    public static readonly TaxYearEndType Fiscal5253 = new("Fiscal 52 or 53", 3);

    private TaxYearEndType(string name, int value) : base(name, value)
    {
    }
}

