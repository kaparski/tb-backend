namespace TaxBeacon.DAL.Documents.Entities;

public class AccountDocument
{
    public Guid TenantId { get; set; }

    public Guid AccountId { get; set; }

    public Guid DocumentId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Account Account { get; set; } = null!;

    public Document Document { get; set; } = null!;
}
