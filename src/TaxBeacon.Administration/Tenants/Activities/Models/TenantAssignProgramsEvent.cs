using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Tenants.Activities.Models;

public sealed class TenantAssignProgramsEvent: EventBase
{
    public TenantAssignProgramsEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        string assignedProgramsNames,
        DateTime assignDateTime)
        : base(executorId, executorRoles, executorFullName)
    {
        AssignedProgramsNames = assignedProgramsNames;
        AssignDateTime = assignDateTime;
    }

    public string AssignedProgramsNames { get; }

    public DateTime AssignDateTime { get; }

    public override string ToString() => $"Access to the following program(s) provided: {AssignedProgramsNames}";
}
