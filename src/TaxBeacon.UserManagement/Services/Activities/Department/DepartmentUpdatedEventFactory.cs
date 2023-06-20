using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Models.Activities;

namespace TaxBeacon.UserManagement.Services.Activities.Department;

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
