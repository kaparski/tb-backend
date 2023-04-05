namespace TaxBeacon.UserManagement.Models.Activities;

public class UserUpdatedEvent: UserEventBase
{
    public Guid UpdatedById { get; }

    public DateTime UpdatedDate { get; }

    public string PreviousValues { get; }

    public string CurrentValues { get; }

    public UserUpdatedEvent(Guid updatedById, DateTime updatedDate, string executorFullName, string executorRoles,
        string previousValues, string currentValues)
        : base(executorFullName, executorRoles)
    {
        UpdatedById = updatedById;
        UpdatedDate = updatedDate;
        PreviousValues = previousValues;
        CurrentValues = currentValues;
    }

    public override string ToString() => $"User details updated: {PreviousValues} to {CurrentValues}";
}
