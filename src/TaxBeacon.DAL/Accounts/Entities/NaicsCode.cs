namespace TaxBeacon.DAL.Accounts.Entities;

public class NaicsCode: BaseDeletableEntity
{
    public int Code { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? ParentCode { get; set; }

    public NaicsCode? Parent { get; set; }

    public ICollection<NaicsCode> Children { get; set; } = new HashSet<NaicsCode>();

    public ICollection<Account> Accounts { get; set; } = new HashSet<Account>();

    public ICollection<Entity> Entities { get; set; } = new HashSet<Entity>();

    public ICollection<Location> Locations { get; set; } = new HashSet<Location>();

}
