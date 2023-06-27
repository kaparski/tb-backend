using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Programs.Activities.Models;

public sealed class ProgramReactivatedEvent: EventBase
{
    public DateTime ReactivatedDate { get; }

    public ProgramReactivatedEvent(Guid executorId, DateTime reactivatedDate, string executorFullName, string executorRoles)
        : base(executorId, executorFullName, executorRoles) => ReactivatedDate = reactivatedDate;

    public override string ToString() => "Program reactivated";
}
