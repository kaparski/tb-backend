using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Models.Activities.Program;

public sealed class ProgramAssignmentUpdatedEvent: EventBase
{
    public DateTime UpdatedDate { get; }

    public string PreviousValues { get; }

    public string CurrentValues { get; }

    public ProgramAssignmentUpdatedEvent(Guid executorId, string executorFullName, string executorRoles, DateTime updatedDate,
        string previousValues, string currentValues)
        : base(executorId, executorFullName, executorRoles)
    {
        UpdatedDate = updatedDate;
        PreviousValues = previousValues;
        CurrentValues = currentValues;
    }

    public override string ToString() => "Program assignment updated";
}

