using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Divisions.Activities.Models;

public class DivisionUpdatedEvent: EventBase
{
    public DivisionUpdatedEvent(Guid executorId, string executorRoles, string executorFullName, DateTime updatedDate,
        string previousValues, string currentValues)
        : base(executorId, executorRoles, executorFullName)
    {
        UpdatedDate = updatedDate;
        PreviousValues = previousValues;
        CurrentValues = currentValues;
    }

    public string CurrentValues { get; set; }

    public string PreviousValues { get; set; }

    public DateTime UpdatedDate { get; set; }

    public override string ToString() => "Divisions details updated";
}