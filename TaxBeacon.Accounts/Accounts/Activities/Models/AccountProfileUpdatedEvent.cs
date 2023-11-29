using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts.Activities.Models;

public class AccountProfileUpdatedEvent: EventBase
{
    public DateTime UpdatedDate { get; }

    public string PreviousValues { get; }

    public string CurrentValues { get; }

    public AccountProfileUpdatedEvent(Guid executorId, string executorFullName, string executorRoles, DateTime updatedDate,
        string previousValues, string currentValues)
        : base(executorId, executorFullName, executorRoles)
    {
        UpdatedDate = updatedDate;
        PreviousValues = previousValues;
        CurrentValues = currentValues;
    }

    public override string ToString() => "Account profile details updated";
}
