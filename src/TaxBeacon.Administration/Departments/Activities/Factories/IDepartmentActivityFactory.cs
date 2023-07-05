using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Departments.Activities.Factories;

public interface IDepartmentActivityFactory
{
    public uint Revision { get; }

    public DepartmentEventType EventType { get; }

    public ActivityItemDto Create(string evt);
}
