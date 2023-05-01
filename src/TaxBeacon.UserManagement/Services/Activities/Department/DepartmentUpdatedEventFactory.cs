using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities;

namespace TaxBeacon.UserManagement.Services.Activities.Department;

public class DepartmentUpdatedEventFactory: IDepartmentActivityFactory
{
    public uint Revision => 1;

    public DepartmentEventType EventType => DepartmentEventType.DepartmentUpdatedEvent;

    public ActivityItemDto Create(string tenantEvent)
    {
        var tenantUpdatedEvent = JsonSerializer.Deserialize<DepartmentUpdatedEvent>(tenantEvent);

        return new ActivityItemDto
        (
            Date: tenantUpdatedEvent!.UpdatedDate,
            FullName: tenantUpdatedEvent.ExecutorFullName,
            Message: tenantUpdatedEvent.ToString()
        );
    }
}
