using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Services.Program.Activities.Models;

public sealed class ProgramOrgUnitAssignEvent: EventBase
{
    public ProgramOrgUnitAssignEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        string departmentName,
        string? serviceAreaName,
        DateTime assignDateTime)
        : base(executorId, executorRoles, executorFullName)
    {
        DepartmentName = departmentName;
        ServiceAreaName = serviceAreaName;
        AssignDateTime = assignDateTime;
    }

    public string DepartmentName { get; }

    public string? ServiceAreaName { get; }

    public DateTime AssignDateTime { get; }

    public override string ToString() =>
        string.IsNullOrEmpty(ServiceAreaName)
            ? $"Program assigned to {DepartmentName} department"
            : $"Program assigned to {DepartmentName} department, {ServiceAreaName} service area";
}
