using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Services.Program.Activities.Models;

public sealed class ProgramUpdatedEvent: EventBase
{
    public DateTime UpdatedDate { get; }

    public string PreviousValues { get; }

    public string CurrentValues { get; }

    public ProgramUpdatedEvent(Guid executorId, string executorFullName, string executorRoles, DateTime updatedDate,
        string previousValues, string currentValues)
        : base(executorId, executorFullName, executorRoles)
    {
        UpdatedDate = updatedDate;
        PreviousValues = previousValues;
        CurrentValues = currentValues;
    }

    public override string ToString() => "Program details updated";
}

