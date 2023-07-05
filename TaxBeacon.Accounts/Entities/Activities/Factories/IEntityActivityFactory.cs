using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Factories;

public interface IEntityActivityFactory
{
    public uint Revision { get; }

    public EntityEventType EventType { get; }

    public ActivityItemDto Create(string evt);
}
