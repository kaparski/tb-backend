using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Accounts.Activities.Factories;

public class EntitiesImportFailedEventFactory: IActivityFactory<AccountEventType>
{
    public uint Revision => 1;

    public AccountEventType EventType => AccountEventType.EntitiesImportFailed;

    public ActivityItemDto Create(string json)
    {
        var evt = JsonSerializer.Deserialize<EntitiesImportFailedEvent>(json);

        return new ActivityItemDto
        (
            Date: evt!.ImportDate,
            FullName: evt.ExecutorFullName,
            Message: evt.ToString()
        );
    }
}
