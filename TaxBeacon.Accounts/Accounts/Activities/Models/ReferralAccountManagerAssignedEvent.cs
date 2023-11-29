using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts.Activities.Models;

public sealed class ReferralAccountManagerAssignedEvent: EventBase
{
    public DateTime AssignedDate { get; }

    public string AccountManagers { get; }


    public ReferralAccountManagerAssignedEvent(Guid executorId, string executorRoles, string executorFullName,
        DateTime assignedDate, string accountManagers): base(executorId, executorRoles, executorFullName)
    {
        AssignedDate = assignedDate;
        AccountManagers = accountManagers;
    }

    public override string ToString() => $"Referral Account assigned to Account Manager(s) {AccountManagers}";
}
