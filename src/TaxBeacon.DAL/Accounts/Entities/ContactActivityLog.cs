using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;
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
