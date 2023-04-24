using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Models.Activities.Tenant;

public sealed class TenantEnteredEvent: EventBase
{
    public TenantEnteredEvent(Guid executorId, string executorRoles, string executorFullName, DateTime enteredDate)
        : base(executorId, executorRoles, executorFullName) =>
        EnteredDate = enteredDate;

    public DateTime EnteredDate { get; set; }

    public override string ToString() => "User entered the tenant";
}
