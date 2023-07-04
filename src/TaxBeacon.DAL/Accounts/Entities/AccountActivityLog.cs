using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;

public class AccountActivityLog
{
    public Guid TenantId { get; set; }

    public Guid AccountId { get; set; }

    public DateTime Date { get; set; }

    public AccountEventType EventType { get; set; }

    public AccountPartActivityType AccountPartType { get; set; }

    public uint Revision { get; set; }

    public string Event { get; set; } = null!;

    public Account Account { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
