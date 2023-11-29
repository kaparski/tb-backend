using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Contacts.Activities.Models;

public sealed class ContactAssignedToAccountEvent: EventBase
{
    public DateTime AssignDate { get; init; }

    public Guid AccountId { get; init; }

    public string AccountName { get; init; }

    public ContactAssignedToAccountEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        DateTime assignDate,
        Guid accountId,
        string accountName) : base(executorId, executorRoles, executorFullName)
    {
        AssignDate = assignDate;
        AccountId = accountId;
        AccountName = accountName;
    }

    public override string ToString() => $"Contact associated with the account: {AccountName}";
}
