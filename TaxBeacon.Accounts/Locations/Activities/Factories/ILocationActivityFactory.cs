using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Locations.Activities.Factories;

// TODO: Make a generic interface for all factories
public interface ILocationActivityFactory
{
    public uint Revision { get; }

    public LocationEventType EventType { get; }

    public ActivityItemDto Create(string locationEvent);
}
