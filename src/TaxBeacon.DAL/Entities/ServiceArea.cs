namespace TaxBeacon.DAL.Entities;

public class ServiceArea: BaseEntity
{
    public Guid TenantId { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Tenant Tenant { get; set; } = null!;

    public ICollection<User> Users { get; set; } = new HashSet<User>();

    public Guid? DepartmentId { get; set; }

    public Department? Department { get; set; }

    public ICollection<ServiceAreaActivityLog> ServiceAreaActivityLogs { get; set; } =
        new HashSet<ServiceAreaActivityLog>();

    public ICollection<ServiceAreaTenantProgram> ServiceAreaTenantPrograms { get; set; } =
        new HashSet<ServiceAreaTenantProgram>();
}