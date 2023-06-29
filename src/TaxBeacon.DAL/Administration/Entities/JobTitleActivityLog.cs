using TaxBeacon.Common.Enums.Activities;

namespace TaxBeacon.DAL.Administration.Entities;

public class JobTitleActivityLog
{
    public Guid TenantId { get; set; }

    public Guid JobTitleId { get; set; }

    public DateTime Date { get; set; }

    public JobTitleEventType EventType { get; set; }

    public uint Revision { get; set; }

    public string Event { get; set; } = string.Empty;

    public JobTitle JobTitle { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
