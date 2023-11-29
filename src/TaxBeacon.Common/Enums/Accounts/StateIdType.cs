using Ardalis.SmartEnum;

namespace TaxBeacon.Common.Enums.Accounts;
public class StateIdType: SmartEnum<StateIdType>
{
    public static readonly StateIdType None = new("None", 0);
    public static readonly StateIdType TaxId = new("Tax ID", 1);
    public static readonly StateIdType RegistrationId = new("Registration ID", 2);

    private StateIdType(string name, int value) : base(name, value)
    {
    }
}
