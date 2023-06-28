using TaxBeacon.Common.Enums.Activities;

namespace TaxBeacon.DAL.Administration.Entities;

public class DivisionActivityLog
{
    public Guid TenantId { get; set; }

    public Guid DivisionId { get; set; }

    public DateTime Date { get; set; }

    public DivisionEventType EventType { get; set; }

    public uint Revision { get; set; }

    public string Event { get; set; } = string.Empty;

    public Division Division { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
