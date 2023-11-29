using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Models;

public sealed class StateIdAddedEvent: EventBase
{
    public DateTime CreatedDate { get; }
    public List<string> StateIdCodes { get; }

    public StateIdAddedEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        DateTime createdDate,
        List<string> stateIdCodes): base(executorId, executorRoles, executorFullName)
    {
        CreatedDate = createdDate;
        StateIdCodes = stateIdCodes;
    }

    public override string ToString() => $"State ID(s) {string.Join(", ", StateIdCodes)} added";
}
