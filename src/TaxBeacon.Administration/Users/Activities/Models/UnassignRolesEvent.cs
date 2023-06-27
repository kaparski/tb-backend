namespace TaxBeacon.Administration.Users.Activities.Models;

public class UnassignRolesEvent: UserEventBase
{
    public UnassignRolesEvent(string unassignedRoles, DateTime unassignDate, Guid executorId, string executorFullName, string executorRoles)
        : base(executorId, executorRoles, executorFullName)
    {
        UnassignDate = unassignDate;
        UnassignedRoles = unassignedRoles;
    }

    public DateTime UnassignDate { get; set; }

    public string UnassignedRoles { get; set; }

    public override string ToString() => $"User unassigned from the following role(s): {UnassignedRoles}";
}
