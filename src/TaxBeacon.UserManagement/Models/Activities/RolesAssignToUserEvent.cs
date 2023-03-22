using TaxBeacon.Common.Services;

namespace TaxBeacon.UserManagement.Models.Activities;

public class RolesAssignToUserEvent
{
    public RolesAssignToUserEvent(string roles, Guid assignedUserId, Guid assignedByUserId, DateTime assignDate)
    {
        Roles = roles;
        AssignedUserId = assignedUserId;
        AssignedByUserId = assignedByUserId;
        AssignDate = assignDate;
    }

    public string Roles { get; set; }

    public Guid AssignedUserId { get; set; }

    public Guid AssignedByUserId { get; set; }

    public DateTime AssignDate { get; set; }

    public string ToString(IDateTimeFormatter dateTimeFormatter) => $"User was assigned to the following roles: {Roles} by {dateTimeFormatter.FormatDate(AssignDate)}";
}
