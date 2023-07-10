using Ardalis.SmartEnum;

namespace TaxBeacon.Common.Enums.Accounts;

public class PhoneType: SmartEnum<PhoneType>
{
    public static readonly PhoneType Office = new("Office", 0);
    public static readonly PhoneType Mobile = new("Mobile", 1);
    public static readonly PhoneType Fax = new("Fax", 2);

    public PhoneType(string name, int value) : base(name, value)
    {
    }
}
