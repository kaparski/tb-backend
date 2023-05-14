using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Models.Activities.JobTitle;

public sealed class JobTitleUpdatedEvent: EventBase
{
    public JobTitleUpdatedEvent(Guid executorId, string executorRoles, string executorFullName, DateTime updatedDate,
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

    public override string ToString() => "Job Title details updated";
}

