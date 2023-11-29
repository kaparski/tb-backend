namespace TaxBeacon.DAL.Administration.Entities;

public class Team: BaseDeletableEntity
{
    public Guid TenantId { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public ICollection<User> Users { get; set; } = new HashSet<User>();

    public ICollection<TeamActivityLog> TeamActivityLogs { get; set; } = new HashSet<TeamActivityLog>();
}
