using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Divisions.Activities.Factories;

public interface IDivisionActivityFactory
{
    public uint Revision { get; }

    public DivisionEventType EventType { get; }

    public ActivityItemDto Create(string userEvent);
}
