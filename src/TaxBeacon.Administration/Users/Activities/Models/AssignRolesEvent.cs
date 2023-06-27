
namespace TaxBeacon.Administration.Users.Activities.Models;

public class AssignRolesEvent: UserEventBase
{
    public AssignRolesEvent(string assignedRoles, DateTime assignDate, Guid executorId, string executorFullName, string executorRoles)
        : base(executorId, executorRoles, executorFullName)
    {
        AssignDate = assignDate;
        AssignedRoles = assignedRoles;
    }

    public DateTime AssignDate { get; set; }

    public string AssignedRoles { get; set; }

    public override string ToString() => $"User assigned to the following role(s): {AssignedRoles}";
}
