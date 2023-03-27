using TaxBeacon.Common.Services;

namespace TaxBeacon.UserManagement.Models.Activities;

public class UserUpdatedEvent
{
    public Guid UpdatedById { get; }

    public DateTime UpdatedDate { get; }

    public string FullName { get; }

    public string Roles { get; }

    public string PreviousValues { get; }

    public string CurrentValues { get; }

    public UserUpdatedEvent(Guid updatedById, DateTime updatedDate, string fullName, string roles,
        string previousValues, string currentValues)
    {
        UpdatedById = updatedById;
        UpdatedDate = updatedDate;
        FullName = fullName;
        Roles = roles;
        PreviousValues = previousValues;
        CurrentValues = currentValues;
    }

    public string ToString(IDateTimeFormatter dateTimeFormatter) =>
        $"User updated {dateTimeFormatter.FormatDate(UpdatedDate)} by {FullName} {Roles}";
}
