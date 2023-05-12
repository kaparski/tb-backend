using TaxBeacon.Common.Enums.Activities;

namespace TaxBeacon.DAL.Entities;

public class ServiceAreaActivityLog
{
    public Guid TenantId { get; set; }

    public Guid ServiceAreaId { get; set; }

    public DateTime Date { get; set; }

    public ServiceAreaEventType EventType { get; set; }

    public uint Revision { get; set; }

    public string Event { get; set; } = string.Empty;

    public ServiceArea ServiceArea { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
