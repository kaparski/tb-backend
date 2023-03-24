
namespace TaxBeacon.UserManagement.Models.Activities;

public class AssignRolesEvent
{
    public AssignRolesEvent(string roles, Guid assignedByUserId, string fullName, List<RoleActivityDto> previousUserRoles, List<RoleActivityDto> currentUserRoles)
    {
        Roles = roles;
        AssignedByUserId = assignedByUserId;
        FullName = fullName;
        PreviousUserRoles = previousUserRoles;
        CurrentUserRoles = currentUserRoles;
    }

    public Guid AssignedByUserId { get; set; }

    public string FullName { get; set; }

    public string Roles { get; set; }

    public List<RoleActivityDto> PreviousUserRoles { get; set; }

    public List<RoleActivityDto> CurrentUserRoles { get; set; }

    public string ToString() => $"User was assigned to the following roles: {Roles} by {FullName}";
}
