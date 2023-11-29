namespace TaxBeacon.DAL.Administration.Entities;

public class Department: BaseDeletableEntity
{
    public Guid TenantId { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? DivisionId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Division? Division { get; set; }

    public ICollection<User> Users { get; set; } = new HashSet<User>();

    public ICollection<JobTitle> JobTitles { get; set; } = new HashSet<JobTitle>();

    public ICollection<ServiceArea> ServiceAreas { get; set; } = new HashSet<ServiceArea>();

    public ICollection<DepartmentActivityLog> DepartmentActivityLogs { get; set; } =
        new HashSet<DepartmentActivityLog>();

    public ICollection<DepartmentTenantProgram> DepartmentTenantPrograms { get; set; } =
        new HashSet<DepartmentTenantProgram>();
}
