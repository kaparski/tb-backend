using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models.Activities.Dtos;

namespace TaxBeacon.UserManagement.Models.Activities;

public class UnassignUsersEvent
{

    public UnassignUsersEvent(string unassignedRoles, Guid unassignedByUserId, string unassignedByFullName, DateTime unassignDate,
        List<RoleActivityDto> activityLogRoleDtos)
    {
        ActivityLogRoleDtos = activityLogRoleDtos;
        UnassignDate = unassignDate;
        UnassignedRoles = unassignedRoles;
        UnassignedByUserId = unassignedByUserId;
        UnassignedByFullName = unassignedByFullName;
    }

    public DateTime UnassignDate { get; set; }

    public string UnassignedByFullName { get; set; }

    public Guid UnassignedByUserId { get; set; }

    public string UnassignedRoles { get; set; }

    public List<RoleActivityDto> ActivityLogRoleDtos { get; set; }

    public override string ToString() => $"User has been unassigned to the following role(s): {UnassignedRoles}";
}
