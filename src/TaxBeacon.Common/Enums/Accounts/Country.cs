using Ardalis.SmartEnum;

namespace TaxBeacon.Common.Enums.Accounts;

public class Country: SmartEnum<Country>
{
    public static readonly Country UnitedStates = new("United States", 1);
    public static readonly Country International = new("International", 2);

    public Country(string name, int value) : base(name, value)
    {
    }
}
