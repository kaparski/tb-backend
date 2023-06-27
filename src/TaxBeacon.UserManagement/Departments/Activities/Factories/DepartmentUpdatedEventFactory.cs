using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Departments.Activities.Models;

namespace TaxBeacon.UserManagement.Departments.Activities.Factories;

public class DepartmentUpdatedEventFactory: IDepartmentActivityFactory
{
    public uint Revision => 1;

    public DepartmentEventType EventType => DepartmentEventType.DepartmentUpdatedEvent;

    public ActivityItemDto Create(string json)
    {
        var evt = JsonSerializer.Deserialize<DepartmentUpdatedEvent>(json);

        return new ActivityItemDto
        (
            Date: evt!.UpdatedDate,
            FullName: evt.ExecutorFullName,
            Message: evt.ToString()
        );
    }
}
