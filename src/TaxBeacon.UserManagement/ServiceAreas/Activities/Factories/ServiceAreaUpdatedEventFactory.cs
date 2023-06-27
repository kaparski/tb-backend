using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.ServiceAreas.Activities.Models;

namespace TaxBeacon.UserManagement.ServiceAreas.Activities.Factories;

public class ServiceAreaUpdatedEventFactory: IServiceAreaActivityFactory
{
    public uint Revision => 1;

    public ServiceAreaEventType EventType => ServiceAreaEventType.ServiceAreaUpdatedEvent;

    public ActivityItemDto Create(string activityEvent)
    {
        var serviceAreaUpdatedEvent = JsonSerializer.Deserialize<ServiceAreaUpdatedEvent>(activityEvent);

        return new ActivityItemDto
        (
            Date: serviceAreaUpdatedEvent!.UpdatedDate,
            FullName: serviceAreaUpdatedEvent.ExecutorFullName,
            Message: serviceAreaUpdatedEvent.ToString()
        );
    }
}
