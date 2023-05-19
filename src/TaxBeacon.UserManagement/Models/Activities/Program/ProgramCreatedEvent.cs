using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Models.Activities.Program;

public sealed class ProgramCreatedEvent: EventBase
{
    public DateTime CreatedDate { get; }

    public ProgramCreatedEvent(Guid executorId, DateTime createdDate, string executorFullName, string executorRoles)
        : base(executorId, executorRoles, executorFullName) =>
        CreatedDate = createdDate;

    public override string ToString()
        => "Program created";
}

