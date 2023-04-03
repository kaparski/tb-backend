using TaxBeacon.Common.Services;

namespace TaxBeacon.UserManagement.Models.Activities;

public class AssignRolesEvent
{
    public AssignRolesEvent(string assignedRoles, Guid assignedByUserId, string assignedByFullName, DateTime assignedDate)
    {
        AssignedDate = assignedDate;
        AssignedAssignedRoles = assignedRoles;
        AssignedByUserId = assignedByUserId;
        AssignedByAssignedByFullName = assignedByFullName;
    }

    public DateTime AssignedDate { get; set; }

    public Guid AssignedByUserId { get; set; }

    public string AssignedByAssignedByFullName { get; set; }

    public string AssignedAssignedRoles { get; set; }

    public string ToString(IDateTimeFormatter dateTimeFormatter) => $"User was assigned to the following roles: {AssignedAssignedRoles} by {AssignedByAssignedByFullName} "
                                                                    + $"{dateTimeFormatter.FormatDate(AssignedDate)}";
}
