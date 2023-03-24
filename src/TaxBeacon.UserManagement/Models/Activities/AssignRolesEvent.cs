using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;

namespace TaxBeacon.UserManagement.Models.Activities;

public class AssignRolesEvent
{
    public AssignRolesEvent(string roles, Guid assignedByUserId, string fullName, List<Role> previousUserRoles, List<Role> currentUserRoles)
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

    public List<Role> PreviousUserRoles { get; set; }

    public List<Role> CurrentUserRoles { get; set; }

    public string ToString() => $"User was assigned to the following roles: {Roles} by {FullName}";
}
