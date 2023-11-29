using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts.Activities.Models;
public sealed class SalespersonUnassignedEvent: EventBase
{
    public DateTime UnassignedDate { get; }

    public string Salespersons { get; set; }

    public SalespersonUnassignedEvent(Guid executorId,
        DateTime unassignedDate,
        string executorFullName,
        string executorRoles,
        string salespersons)
        : base(executorId, executorFullName, executorRoles)
    {
        UnassignedDate = unassignedDate;
        Salespersons = salespersons;
    }

    public override string ToString() => $"Account unassigned from Salesperson(s) {Salespersons}";
}
