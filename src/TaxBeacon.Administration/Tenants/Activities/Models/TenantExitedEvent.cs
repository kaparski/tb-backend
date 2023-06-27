using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Tenants.Activities.Models;

public sealed class TenantExitedEvent: EventBase
{
    public TenantExitedEvent(Guid executorId, string executorRoles, string executorFullName, DateTime exitedDate)
        : base(executorId, executorRoles, executorFullName)
        => ExitedDate = exitedDate;

    public DateTime ExitedDate { get; set; }

    public override string ToString() => "User exited the tenant";
}
