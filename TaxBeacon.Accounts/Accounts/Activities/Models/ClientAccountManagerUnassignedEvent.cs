using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts.Activities.Models;

public sealed class ClientAccountManagerUnassignedEvent: EventBase
{
    public DateTime UnassignedDate { get; }

    public string AccountManagers { get; }

    public ClientAccountManagerUnassignedEvent(Guid executorId,
        DateTime unassignedDate,
        string executorFullName,
        string executorRoles,
        string accountManagers)
        : base(executorId, executorFullName, executorRoles)
    {
        UnassignedDate = unassignedDate;
        AccountManagers = accountManagers;
    }

    public override string ToString() => $"Client Account unassigned from Account Manager(s) {AccountManagers}";
}
