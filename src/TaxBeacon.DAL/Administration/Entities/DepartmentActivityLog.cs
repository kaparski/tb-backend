using TaxBeacon.Common.Enums.Activities;

namespace TaxBeacon.DAL.Administration.Entities;

public class DepartmentActivityLog
{
    public Guid TenantId { get; set; }

    public Guid DepartmentId { get; set; }

    public DateTime Date { get; set; }

    public DepartmentEventType EventType { get; set; }

    public uint Revision { get; set; }

    public string Event { get; set; } = string.Empty;

    public Department Department { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
