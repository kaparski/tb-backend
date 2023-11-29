using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts.Activities.Models;
public sealed class SalespersonAssignedEvent: EventBase
{
    public DateTime AssignedDate { get; }

    public string Salespersons { get; set; }

    public SalespersonAssignedEvent(Guid executorId,
        DateTime assignedDate,
        string executorFullName,
        string executorRoles,
        string salespersons)
        : base(executorId, executorFullName, executorRoles)
    {
        AssignedDate = assignedDate;
        Salespersons = salespersons;
    }

    public override string ToString() => $"Account assigned to Salesperson(s) {Salespersons}";
}
