using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Accounts.Activities.Factories;

public class SalespersonUnassignedEventFactory: IActivityFactory<AccountEventType>
{
    public uint Revision => 1;

    public AccountEventType EventType => AccountEventType.SalespersonUnassigned;

    public ActivityItemDto Create(string accountEvent)
    {
        var salespersonUnassignedEvent = JsonSerializer.Deserialize<SalespersonUnassignedEvent>(accountEvent);

        return new ActivityItemDto
        (
            Date: salespersonUnassignedEvent!.UnassignedDate,
            FullName: salespersonUnassignedEvent.ExecutorFullName,
            Message: salespersonUnassignedEvent.ToString()
        );
    }
}
