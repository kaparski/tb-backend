using TaxBeacon.Common.Services;

namespace TaxBeacon.UserManagement.Models.Activities;

public class AssignRolesEvent
{
    public AssignRolesEvent(string roles, Guid assignedByUserId, string fullName, DateTime assignedDate)
    {
        AssignedDate = assignedDate;
        AssignedRoles = roles;
        AssignedByUserId = assignedByUserId;
        AssignedByFullName = fullName;
    }

    public DateTime AssignedDate { get; set; }

    public Guid AssignedByUserId { get; set; }

    public string AssignedByFullName { get; set; }

    public string AssignedRoles { get; set; }

    public string ToString(IDateTimeFormatter dateTimeFormatter) => $"User was assigned to the following roles: {AssignedRoles} by {AssignedByFullName} "
                                                                    + $"{dateTimeFormatter.FormatDate(AssignedDate)}";
}
