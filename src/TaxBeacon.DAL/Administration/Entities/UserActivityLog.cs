using TaxBeacon.Common.Enums.Administration.Activities;

namespace TaxBeacon.DAL.Administration.Entities;

public class UserActivityLog
{
    public Guid TenantId { get; set; }

    public Guid UserId { get; set; }

    public DateTime Date { get; set; }

    public UserEventType EventType { get; set; }

    public uint Revision { get; set; }

    public string Event { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}