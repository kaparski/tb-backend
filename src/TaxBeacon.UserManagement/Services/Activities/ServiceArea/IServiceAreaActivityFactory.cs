using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Activities.ServiceArea;

public interface IServiceAreaActivityFactory
{
    public uint Revision { get; }

    public ServiceAreaEventType EventType { get; }

    public ActivityItemDto Create(string activityEvent);
}
