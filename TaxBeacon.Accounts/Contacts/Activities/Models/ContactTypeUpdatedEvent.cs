using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Contacts.Activities.Models;

public sealed class ContactTypeUpdatedEvent: EventBase
{
    public DateTime UpdatedDate { get; init; }

    public string PreviousValue { get; init; }

    public string CurrentValue { get; init; }

    public ContactTypeUpdatedEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        DateTime updatedDate, string previousValue, string currentValue)
        : base(executorId, executorRoles, executorFullName)
    {
        UpdatedDate = updatedDate;
        PreviousValue = previousValue;
        CurrentValue = currentValue;
    }

    public override string ToString() => "Contact type updated";
}
