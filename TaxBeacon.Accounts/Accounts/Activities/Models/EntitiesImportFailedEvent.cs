using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts.Activities.Models;

public sealed class EntitiesImportFailedEvent: EventBase
{
    public DateTime ImportDate { get; }

    public int EntitiesCount { get; }

    public EntitiesImportFailedEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        int entitiesCount,
        DateTime importDate)
        : base(executorId, executorRoles, executorFullName)
    {
        EntitiesCount = entitiesCount;
        ImportDate = importDate;
    }

    public override string ToString() => $"Entities import failed";
}

