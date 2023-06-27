using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Programs.Activities.Models;

public sealed class ProgramOrgUnitUnassignEvent: EventBase
{
    public ProgramOrgUnitUnassignEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        string departmentName,
        string? serviceAreaName,
        DateTime unassignDateTime)
        : base(executorId, executorRoles, executorFullName)
    {
        DepartmentName = departmentName;
        ServiceAreaName = serviceAreaName;
        UnassignDateTime = unassignDateTime;
    }

    public string DepartmentName { get; }

    public string? ServiceAreaName { get; }

    public DateTime UnassignDateTime { get; }

    public override string ToString() =>
        string.IsNullOrEmpty(ServiceAreaName)
            ? $"Program unassigned from {DepartmentName} department"
            : $"Program unassigned from {DepartmentName} department, {ServiceAreaName} service area";
}
