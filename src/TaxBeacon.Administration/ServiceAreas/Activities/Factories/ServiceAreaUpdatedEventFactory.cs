using System.Text.Json;
using TaxBeacon.Administration.ServiceAreas.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.ServiceAreas.Activities.Factories;

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
