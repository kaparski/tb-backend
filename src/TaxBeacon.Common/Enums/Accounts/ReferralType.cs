using Ardalis.SmartEnum;

namespace TaxBeacon.Common.Enums.Accounts;

public class ReferralType: SmartEnum<ReferralType>
{

    public static readonly ReferralType Paid = new("Paid", 1);
    public static readonly ReferralType NotPaid = new("Not Paid", 2);

    private ReferralType(string name, int value) : base(name, value)
    {
    }
}
