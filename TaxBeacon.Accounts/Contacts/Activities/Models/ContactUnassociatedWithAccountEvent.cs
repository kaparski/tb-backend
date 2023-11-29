using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Contacts.Activities.Models;

public sealed class ContactUnassociatedWithAccountEvent: EventBase
{
    public DateTime UnassociateDate { get; init; }

    public Guid AccountId { get; init; }

    public string AccountName { get; init; }

    public ContactUnassociatedWithAccountEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        DateTime unassociateDate,
        Guid accountId,
        string accountName) : base(executorId, executorRoles, executorFullName)
    {
        UnassociateDate = unassociateDate;
        AccountId = accountId;
        AccountName = accountName;
    }

    public override string ToString() => $"Contact unassociated from the account: {AccountName}";
}
