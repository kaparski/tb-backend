using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;
public class EntityActivityLog
{
    public Guid TenantId { get; set; }

    public Guid EntityId { get; set; }

    public DateTime Date { get; set; }

    public EntityEventType EventType { get; set; }

    public uint Revision { get; set; }

    public string Event { get; set; } = string.Empty;

    public Entity Entity { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
