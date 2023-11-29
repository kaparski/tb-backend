using Ardalis.SmartEnum;

namespace TaxBeacon.Common.Enums;

public class SearchEntityType: SmartEnum<SearchEntityType>
{
    public static readonly SearchEntityType Tenant = new("tenant", 0);
    public static readonly SearchEntityType User = new("user", 1);
    public static readonly SearchEntityType Role = new("role", 2);
    public static readonly SearchEntityType Program = new("program", 3);
    public static readonly SearchEntityType Division = new("division", 4);
    public static readonly SearchEntityType Department = new("department", 5);
    public static readonly SearchEntityType ServiceArea = new("service area", 6);
    public static readonly SearchEntityType Team = new("team", 7);
    public static readonly SearchEntityType JobTitle = new("job title", 8);
    public static readonly SearchEntityType ClientProspect = new("client prospect", 9);
    public static readonly SearchEntityType Client = new("client", 10);
    public static readonly SearchEntityType Entity = new("entity", 11);
    public static readonly SearchEntityType Location = new("location", 12);
    public static readonly SearchEntityType Contact = new("contact", 13);

    private SearchEntityType(string name, int value) : base(name, value)
    {
    }
}
