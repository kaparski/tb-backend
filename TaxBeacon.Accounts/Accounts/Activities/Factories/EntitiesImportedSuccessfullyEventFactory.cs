using System.Text.Json;
using TaxBeacon.Accounts.Accounts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Accounts.Activities.Factories;

public class EntitiesImportedSuccessfullyEventFactory: IActivityFactory<AccountEventType>
{
    public uint Revision => 1;

    public AccountEventType EventType => AccountEventType.EntitiesImportedSuccessfully;

    public ActivityItemDto Create(string json)
    {
        var evt = JsonSerializer.Deserialize<EntitiesImportedSuccessfullyEvent>(json);

        return new ActivityItemDto
        (
            Date: evt!.ImportDate,
            FullName: evt.ExecutorFullName,
            Message: evt.ToString()
        );
    }
}
