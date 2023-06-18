using Ardalis.SmartEnum;

namespace TaxBeacon.Common.Accounts;

public class ReferralState: SmartEnum<ReferralState>
{
    public static readonly ReferralState None = new("None", 0);
    public static readonly ReferralState ReferralProspect = new("Referral prospect", 1);
    public static readonly ReferralState ReferralPartner = new("Referral partner", 2);

    private ReferralState(string name, int value) : base(name, value)
    {
    }
}
