using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Divisions.Activities.Factories;

public interface IDivisionActivityFactory
{
    public uint Revision { get; }

    public DivisionEventType EventType { get; }

    public ActivityItemDto Create(string userEvent);
}
