using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities.ServiceArea;

namespace TaxBeacon.UserManagement.Services.Activities.ServiceArea;

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
