namespace TaxBeacon.DAL.Administration.Entities;

public class ServiceAreaTenantProgram: IDeletableEntity
{
    public Guid TenantId { get; set; }

    public Guid ServiceAreaId { get; set; }

    public Guid ProgramId { get; set; }

    public ServiceArea ServiceArea { get; set; } = null!;

    public TenantProgram TenantProgram { get; set; } = null!;

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedDateTimeUtc { get; set; }
}
