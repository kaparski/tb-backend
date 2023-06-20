using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Services.Entities.Activities;
public interface IEntityActivityFactory
{
    public uint Revision { get; }

    public EntityEventType EventType { get; }

    public ActivityItemDto Create(string evt);
}
