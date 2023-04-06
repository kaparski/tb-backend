using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models.Activities.Dtos;

namespace TaxBeacon.UserManagement.Models.Activities;

public class UnassignUsersEvent
{

    public UnassignUsersEvent(string unassignedRoles, Guid unassignedByUserId, string unassignedByFullName, DateTime unassignDate,
        List<ActivityLogRoleDto> activityLogRoleDtos)
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

    public List<ActivityLogRoleDto> ActivityLogRoleDtos { get; set; }

    public string ToString(IDateTimeFormatter dateTimeFormatter) => $"User was unassigned from the following role(s): {UnassignedRoles}";
}
