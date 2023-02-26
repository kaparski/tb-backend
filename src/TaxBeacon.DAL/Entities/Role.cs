namespace TaxBeacon.DAL.Entities;

public class Role: BaseEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public ICollection<TenantRole> TenantRoles { get; set; } = new HashSet<TenantRole>();
}
