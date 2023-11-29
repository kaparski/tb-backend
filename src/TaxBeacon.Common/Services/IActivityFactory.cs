using TaxBeacon.Common.Models;

namespace TaxBeacon.Common.Services;

public interface IActivityFactory<out T> where T : Enum
{
    public uint Revision { get; }

    public T EventType { get; }

    public ActivityItemDto Create(string eventData);
}
