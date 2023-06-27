using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Departments.Activities.Factories;

public interface IDepartmentActivityFactory
{
    public uint Revision { get; }

    public DepartmentEventType EventType { get; }

    public ActivityItemDto Create(string evt);
}
