namespace TaxBeacon.DAL.Administration.Entities;

public class DepartmentTenantProgram: IDeletableEntity
{
    public Guid TenantId { get; set; }

    public Guid DepartmentId { get; set; }

    public Guid ProgramId { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedDateTimeUtc { get; set; }

    public Department Department { get; set; } = null!;

    public TenantProgram TenantProgram { get; set; } = null!;
}
