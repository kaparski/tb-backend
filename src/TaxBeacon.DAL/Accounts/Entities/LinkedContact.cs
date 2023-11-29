using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;

public class LinkedContact
{
    public Guid TenantId { get; set; }

    public Guid SourceContactId { get; set; }

    public Guid RelatedContactId { get; set; }

    public string Comment { get; set; } = null!;

    public Contact SourceContact { get; set; } = null!;

    public Contact RelatedContact { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
