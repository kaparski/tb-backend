using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models.Activities.Dtos;

namespace TaxBeacon.UserManagement.Models.Activities;

public class AssignRolesEvent
{
    public AssignRolesEvent(string assignedRoles, Guid assignedByUserId, string assignedByFullName, DateTime assignedDate, List<ActivityLogRoleDto> activityLogRoleDtos)
    {
        AssignedDate = assignedDate;
        ActivityLogRoleDtos = activityLogRoleDtos;
        AssignedRoles = assignedRoles;
        AssignedByUserId = assignedByUserId;
        AssignedByFullName = assignedByFullName;
    }

    public DateTime AssignedDate { get; set; }

    public Guid AssignedByUserId { get; set; }

    public string AssignedByFullName { get; set; }

    public string AssignedRoles { get; set; }

    public List<ActivityLogRoleDto> ActivityLogRoleDtos { get; set; }

    public string ToString(IDateTimeFormatter dateTimeFormatter) => $"User was assigned to the following role(s): {AssignedRoles}";
}
