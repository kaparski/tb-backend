using TaxBeacon.Common.Enums.Administration.Activities;

namespace TaxBeacon.DAL.Administration.Entities;

public class ProgramActivityLog
{
    public Guid? TenantId { get; set; }

    public Guid ProgramId { get; set; }

    public DateTime Date { get; set; }

    public ProgramEventType EventType { get; set; }

    public uint Revision { get; set; }

    public string Event { get; set; } = string.Empty;

    public Program Program { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
