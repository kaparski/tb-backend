using TaxBeacon.Common.Enums.Activities;

namespace TaxBeacon.DAL.Entities.Accounts;
public class ContactActivityLog
{
    public Guid TenantId { get; set; }

    public Guid ContactId { get; set; }

    public DateTime Date { get; set; }

    public ContactEventType EventType { get; set; }

    public uint Revision { get; set; }

    public string Event { get; set; } = string.Empty;

    public Contact Contact { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
