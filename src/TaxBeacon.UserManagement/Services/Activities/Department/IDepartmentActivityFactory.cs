using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Activities.Department;

public interface IDepartmentActivityFactory
{
    public uint Revision { get; }

    public DepartmentEventType EventType { get; }

    public ActivityItemDto Create(string evt);
}