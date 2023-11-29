using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts.Activities.Models;

public sealed class ClientAccountManagerAssignedEvent: EventBase
{
    public DateTime AssignedDate { get; }

    public string AccountManagers { get; }

    public ClientAccountManagerAssignedEvent(Guid executorId,
        DateTime assignedDate,
        string executorFullName,
        string executorRoles,
        string accountManagers)
        : base(executorId, executorFullName, executorRoles)
    {
        AssignedDate = assignedDate;
        AccountManagers = accountManagers;
    }

    public override string ToString() => $"Client Account assigned to Account Manager(s) {AccountManagers}";
}
