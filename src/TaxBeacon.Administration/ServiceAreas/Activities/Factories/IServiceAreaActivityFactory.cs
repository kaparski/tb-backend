using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.ServiceAreas.Activities.Factories;

public interface IServiceAreaActivityFactory
{
    public uint Revision { get; }

    public ServiceAreaEventType EventType { get; }

    public ActivityItemDto Create(string activityEvent);
}
