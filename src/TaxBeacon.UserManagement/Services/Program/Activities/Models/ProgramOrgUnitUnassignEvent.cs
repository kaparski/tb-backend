using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Services.Program.Activities.Models;

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
            ? $"Program assigned from {DepartmentName} department"
            : $"Program assigned from {DepartmentName} department, {ServiceAreaName} service area";
}
