using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Models;

public sealed class EntityImportedEvent: EventBase
{
    public DateTime CreatedDate { get; }

    public EntityImportedEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        DateTime createdDate): base(executorId, executorRoles, executorFullName) =>
        CreatedDate = createdDate;

    public override string ToString() => "Entity created via import";
}
