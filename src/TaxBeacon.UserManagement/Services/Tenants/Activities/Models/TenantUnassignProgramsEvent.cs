using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Services.Tenants.Activities.Models;

public sealed class TenantUnassignProgramsEvent: EventBase
{
    public TenantUnassignProgramsEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        string unassignedProgramsNames,
        DateTime unassignDateTime)
        : base(executorId, executorRoles, executorFullName)
    {
        UnassignDateTime = unassignDateTime;
        UnassignedProgramsNames = unassignedProgramsNames;
    }

    public DateTime UnassignDateTime { get; }

    public string UnassignedProgramsNames { get; }

    public override string ToString() => $"Access to the following program(s) removed: {UnassignedProgramsNames}";
}
