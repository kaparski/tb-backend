namespace TaxBeacon.DAL.Documents.Entities;

public class Document: BaseDeletableEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Url { get; set; } = null!;

    public long ContentLength { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public TenantUser TenantUser { get; set; } = null!;

    public ICollection<AccountDocument> AccountDocuments { get; set; } = new HashSet<AccountDocument>();

    public ICollection<EntityDocument> EntityDocuments { get; set; } = new HashSet<EntityDocument>();

    public ICollection<LocationDocument> LocationDocuments { get; set; } = new HashSet<LocationDocument>();
}
