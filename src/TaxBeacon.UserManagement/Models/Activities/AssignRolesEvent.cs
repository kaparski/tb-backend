namespace TaxBeacon.UserManagement.Models.Activities;

public sealed class AssignRolesEvent: UserEventBase
{
    public AssignRolesEvent(string executorRoles, Guid executorId, string executorFullName, List<RoleActivityDto> previousUserRoles, List<RoleActivityDto> currentUserRoles, DateTime assignDate)
        : base(executorId, executorRoles, executorFullName)
    {
        PreviousUserRoles = previousUserRoles;
        CurrentUserRoles = currentUserRoles;
        AssignDate = assignDate;
    }

    public DateTime AssignDate { get; }

    public List<RoleActivityDto> PreviousUserRoles { get; }

    public List<RoleActivityDto> CurrentUserRoles { get; }

    public override string ToString()
        => $"User has been assigned to the following role(s): {string.Join(", ", CurrentUserRoles.Select(x => x.Name))}";
}
