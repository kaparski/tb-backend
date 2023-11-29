namespace TaxBeacon.DAL.Documents.Entities;

public class EntityDocument
{
    public Guid TenantId { get; set; }

    public Guid EntityId { get; set; }

    public Guid DocumentId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Entity Entity { get; set; } = null!;

    public Document Document { get; set; } = null!;
}
