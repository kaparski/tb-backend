using TaxBeacon.Common.Services;

namespace TaxBeacon.UserManagement.Models.Activities;

public class AssignRolesEvent
{
    public AssignRolesEvent(string roles, Guid assignedByUserId, string fullName, List<RoleActivityDto> previousUserRoles, List<RoleActivityDto> currentUserRoles, DateTime assignDate)
    {
        Roles = roles;
        AssignedByUserId = assignedByUserId;
        FullName = fullName;
        PreviousUserRoles = previousUserRoles;
        CurrentUserRoles = currentUserRoles;
        AssignDate = assignDate;
    }

    public Guid AssignedByUserId { get; }

    public string FullName { get; }

    public string Roles { get; }

    public DateTime AssignDate { get; }

    public List<RoleActivityDto> PreviousUserRoles { get; }

    public List<RoleActivityDto> CurrentUserRoles { get; }

    public string ToString(IDateTimeFormatter dateTimeFormatter)
        => $"{dateTimeFormatter.FormatDate(AssignDate)} User has been assigned to the following roles: {string.Join(", ", CurrentUserRoles.Select(x => x.Name))} by {FullName} {Roles}";
}
