using TaxBeacon.Common.Enums.Activities;

namespace TaxBeacon.DAL.Entities;

public class DivisionActivityLog
{
    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public DateTime Date { get; set; }

    public DivisionEventType EventType { get; set; }

    public uint Revision { get; set; }

    public string Event { get; set; } = string.Empty;
}
