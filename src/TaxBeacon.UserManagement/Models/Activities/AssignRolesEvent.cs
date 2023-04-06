using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models.Activities.Dtos;

namespace TaxBeacon.UserManagement.Models.Activities;

public class AssignRolesEvent
{
    public AssignRolesEvent(string assignedRoles, Guid assignedByUserId, string executorFullName, DateTime assignDate, List<RoleActivityDto> activityLogRoleDtos)
    {
        AssignDate = assignDate;
        ActivityLogRoleDtos = activityLogRoleDtos;
        AssignedRoles = assignedRoles;
        AssignedByUserId = assignedByUserId;
        ExecutorFullName = executorFullName;
    }

    public DateTime AssignDate { get; set; }

    public Guid AssignedByUserId { get; set; }

    public string ExecutorFullName { get; set; }

    public string AssignedRoles { get; set; }

    public List<RoleActivityDto> ActivityLogRoleDtos { get; set; }

    public override string ToString() => $"User has been assigned to the following role(s): {AssignedRoles}";
}
