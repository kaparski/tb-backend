namespace TaxBeacon.DAL.Documents.Entities;

public class LocationDocument
{
    public Guid TenantId { get; set; }

    public Guid LocationId { get; set; }

    public Guid DocumentId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Location Location { get; set; } = null!;

    public Document Document { get; set; } = null!;
}
