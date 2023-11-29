using Ardalis.SmartEnum;

namespace TaxBeacon.Common.Enums.Accounts;
public class ContactType: SmartEnum<ContactType>
{
    public static readonly ContactType None = new("None", 0);
    public static readonly ContactType Client = new("Employee", 1);
    public static readonly ContactType ReferralPartner = new("Contractor", 2);
    public static readonly ContactType ReferringIndividual = new("Referring Individual", 3);

    private ContactType(string name, int value) : base(name, value)
    {
    }
}
