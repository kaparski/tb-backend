using System.Text.Json;

namespace TaxBeacon.UserManagement.Models.Activities;

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

    public override string ToString()
    {
        var currentValues = JsonSerializer.Deserialize<UpdateUserDto>(CurrentValues) ?? new UpdateUserDto();
        return $"User details updated: {currentValues}";
    }
}

