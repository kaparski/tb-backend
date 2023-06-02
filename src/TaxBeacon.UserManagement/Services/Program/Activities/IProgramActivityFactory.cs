using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Program.Activities;

public interface IProgramActivityFactory
{
    public uint Revision { get; }

    public ProgramEventType EventType { get; }

    public ActivityItemDto Create(string programEvent);
}