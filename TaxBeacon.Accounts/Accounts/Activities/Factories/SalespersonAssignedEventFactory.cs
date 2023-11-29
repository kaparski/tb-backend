using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Accounts.Activities.Factories;

public class SalespersonAssignedEventFactory: IActivityFactory<AccountEventType>
{
    public uint Revision => 1;

    public AccountEventType EventType => AccountEventType.SalespersonAssigned;

    public ActivityItemDto Create(string accountEvent)
    {
        var salespersonAssignedEvent = JsonSerializer.Deserialize<SalespersonAssignedEvent>(accountEvent);

        return new ActivityItemDto
        (
            Date: salespersonAssignedEvent!.AssignedDate,
            FullName: salespersonAssignedEvent.ExecutorFullName,
            Message: salespersonAssignedEvent.ToString()
        );
    }
}
