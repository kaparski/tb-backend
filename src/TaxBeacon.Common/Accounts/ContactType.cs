using Ardalis.SmartEnum;

namespace TaxBeacon.Common.Accounts;
public class ContactType: SmartEnum<ContactType>
{
    public static readonly ContactType None = new("None", 0);
    public static readonly ContactType Client = new("Client", 1);
    public static readonly ContactType ReferralPartner = new("Referral Partner", 2);
    public static readonly ContactType ClientPartner = new("Client Partner", 3);

    private ContactType(string name, int value) : base(name, value)
    {
    }
}
