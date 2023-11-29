using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Activities.Models;

public class StateIdDeletedEvent: EventBase
{
    public StateIdDeletedEvent(Guid executorId, string executorRoles, string executorFullName, DateTime removedDate,
        string stateIdCode)
        : base(executorId, executorRoles, executorFullName)
    {
        RemovedDate = removedDate;
        StateIdCode = stateIdCode;
    }

    public DateTime RemovedDate { get; set; }

    public string StateIdCode { get; set; }

    public override string ToString() => $"State ID {StateIdCode} deleted";
}
