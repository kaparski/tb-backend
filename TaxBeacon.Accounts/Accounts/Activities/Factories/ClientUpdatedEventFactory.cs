using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts.Activities.Factories;

public class ClientUpdatedEventFactory: IAccountActivityFactory
{
    public uint Revision => 1;

    public AccountEventType EventType => AccountEventType.ClientDetailsUpdated;

    public ActivityItemDto Create(string json)
    {
        var evt = JsonSerializer.Deserialize<ClientUpdatedEvent>(json);

        return new ActivityItemDto
        (
            Date: evt!.UpdatedDate,
            FullName: evt.ExecutorFullName,
            Message: evt.ToString()
        );
    }
}
