
using TaxBeacon.Common.Enums.Administration.Activities;

namespace TaxBeacon.DAL.Administration.Entities;

public class TeamActivityLog
{
    public Guid TenantId { get; set; }

    public Guid TeamId { get; set; }

    public DateTime Date { get; set; }

    public TeamEventType EventType { get; set; }

    public uint Revision { get; set; }

    public string Event { get; set; } = string.Empty;

    public Team Team { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}