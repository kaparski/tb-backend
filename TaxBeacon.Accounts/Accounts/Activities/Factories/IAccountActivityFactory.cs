using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts.Activities.Factories;

public interface IAccountActivityFactory
{
    public uint Revision { get; }

    public AccountEventType EventType { get; }

    public ActivityItemDto Create(string accountEvent);
}
