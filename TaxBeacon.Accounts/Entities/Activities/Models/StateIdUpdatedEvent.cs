using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Models;

public sealed class StateIdUpdatedEvent: EventBase
{
    public DateTime UpdatedDate { get; init; }

    public string StateIdCode { get; init; }

    public StateIdUpdatedEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        string stateIdCode,
        DateTime updatedDate) : base(executorId, executorRoles, executorFullName)
    {
        StateIdCode = stateIdCode;
        UpdatedDate = updatedDate;
    }

    public override string ToString() => $"State ID {StateIdCode} updated";
}
