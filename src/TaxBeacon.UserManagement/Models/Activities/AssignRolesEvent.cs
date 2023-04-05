namespace TaxBeacon.UserManagement.Models.Activities;

public sealed class AssignRolesEvent: UserEventBase
{
    public AssignRolesEvent(string executorRoles, Guid assignedByUserId, string executorFullName, List<RoleActivityDto> previousUserRoles, List<RoleActivityDto> currentUserRoles, DateTime assignDate)
        : base(executorRoles, executorFullName)
    {
        AssignedByUserId = assignedByUserId;
        PreviousUserRoles = previousUserRoles;
        CurrentUserRoles = currentUserRoles;
        AssignDate = assignDate;
    }

    public Guid AssignedByUserId { get; }

    public DateTime AssignDate { get; }

    public List<RoleActivityDto> PreviousUserRoles { get; }

    public List<RoleActivityDto> CurrentUserRoles { get; }

    public override string ToString()
        => $"User has been assigned to the following role(s): {string.Join(", ", CurrentUserRoles.Select(x => x.Name))}";
}
