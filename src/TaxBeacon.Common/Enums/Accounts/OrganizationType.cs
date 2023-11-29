using Ardalis.SmartEnum;

namespace TaxBeacon.Common.Enums.Accounts;

public class OrganizationType: SmartEnum<OrganizationType>
{
    public static readonly OrganizationType Employee = new("Employee", 1);
    public static readonly OrganizationType Client = new("Client", 2);
    public static readonly OrganizationType Contractor = new("Contractor", 3);
    public static readonly OrganizationType ThirdParty = new("Third Party", 4);

    public OrganizationType(string name, int value) : base(name, value)
    {
    }
}
