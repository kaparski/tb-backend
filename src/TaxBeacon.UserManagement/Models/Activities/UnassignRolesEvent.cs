namespace TaxBeacon.UserManagement.Models.Activities;

public class UnassignUsersEvent: UserEventBase
{
    public UnassignUsersEvent(string unassignedRoles, DateTime unassignDate, Guid executorId, string executorFullName, string executorRoles)
        : base(executorId, executorRoles, executorFullName)
    {
        UnassignDate = unassignDate;
        UnassignedRoles = unassignedRoles;
    }

    public DateTime UnassignDate { get; set; }

    public string UnassignedRoles { get; set; }

    public override string ToString() => $"User has been unassigned from the following role(s): {UnassignedRoles}";
}
