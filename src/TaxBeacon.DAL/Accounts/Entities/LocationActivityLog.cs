using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;

public class LocationActivityLog
{
    public Guid TenantId { get; set; }

    public Guid LocationId { get; set; }

    public DateTime Date { get; set; }

    public LocationEventType EventType { get; set; }

    public uint Revision { get; set; }

    public string Event { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;

    public Location Location { get; set; } = null!;
}
