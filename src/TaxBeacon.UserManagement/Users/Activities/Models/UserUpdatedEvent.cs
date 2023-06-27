namespace TaxBeacon.UserManagement.Users.Activities.Models;

public sealed class UserUpdatedEvent: UserEventBase
{
    public DateTime UpdatedDate { get; }

    public string PreviousValues { get; }

    public string CurrentValues { get; }

    public UserUpdatedEvent(Guid executorId, DateTime updatedDate, string executorFullName, string executorRoles,
        string previousValues, string currentValues)
        : base(executorId, executorFullName, executorRoles)
    {
        UpdatedDate = updatedDate;
        PreviousValues = previousValues;
        CurrentValues = currentValues;
    }

    public override string ToString() => "User details updated";
}

