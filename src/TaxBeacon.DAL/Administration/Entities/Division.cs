namespace TaxBeacon.DAL.Administration.Entities;

public class Division: BaseEntity
{
    public Guid TenantId { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Tenant Tenant { get; set; } = null!;

    public ICollection<User> Users { get; set; } = new HashSet<User>();

    public ICollection<Department> Departments { get; set; } = new HashSet<Department>();

    public ICollection<DivisionActivityLog> DivisionActivityLogs { get; set; } = new HashSet<DivisionActivityLog>();

}