using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models.Activities.DivisionsActivities;

namespace TaxBeacon.UserManagement.Services.Activities.DivisionActivityHistory;

public interface IDivisionActivityFactory
{
    public uint Revision { get; }

    public DivisionEventType EventType { get; }

    public DivisionActivityItemDto Create(string userEvent);
}
