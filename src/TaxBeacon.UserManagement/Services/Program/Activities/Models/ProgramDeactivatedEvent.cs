using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Services.Program.Activities.Models;

public sealed class ProgramDeactivatedEvent: EventBase
{
    public DateTime DeactivatedDate { get; }

    public ProgramDeactivatedEvent(Guid executorId, DateTime deactivatedDate, string executorFullName, string executorRoles)
        : base(executorId, executorFullName, executorRoles) => DeactivatedDate = deactivatedDate;

    public override string ToString() => $"Program deactivated";
}
