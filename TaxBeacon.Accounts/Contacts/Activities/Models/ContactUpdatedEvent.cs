using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Contacts.Activities.Models;

public class ContactUpdatedEvent: EventBase
{
    public DateTime UpdatedDate { get; }

    public string CurrentValues { get; set; }

    public string PreviousValues { get; set; }


    public ContactUpdatedEvent(Guid executorId, string executorRoles, string executorFullName, DateTime updatedDate,
        string previousValues, string currentValues)
        : base(executorId, executorRoles, executorFullName)
    {
        UpdatedDate = updatedDate;
        PreviousValues = previousValues;
        CurrentValues = currentValues;
    }

    public override string ToString() => "Contact details updated";
}
