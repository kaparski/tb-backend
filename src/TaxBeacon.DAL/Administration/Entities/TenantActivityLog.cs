using TaxBeacon.Common.Enums.Administration.Activities;

namespace TaxBeacon.DAL.Administration.Entities;

public class TenantActivityLog
{
    public Guid TenantId { get; set; }

    public DateTime Date { get; set; }

    public TenantEventType EventType { get; set; }

    public uint Revision { get; set; }

    public string Event { get; set; } = string.Empty;

    public Tenant Tenant { get; set; } = null!;
}
