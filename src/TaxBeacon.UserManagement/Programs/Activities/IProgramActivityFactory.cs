using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Programs.Activities;

public interface IProgramActivityFactory
{
    public uint Revision { get; }

    public ProgramEventType EventType { get; }

    public ActivityItemDto Create(string programEvent);
}
