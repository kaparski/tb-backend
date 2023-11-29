using System.Text.Json;
using TaxBeacon.Administration.Users.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Users.Activities.Factories;

public sealed class CredentialsSentEventFactory: IUserActivityFactory
{
    public uint Revision => 1;

    public UserEventType UserEventType => UserEventType.CredentialSent;

    public ActivityItemDto Create(string userEvent)
    {
        var credentialsSentEvent = JsonSerializer.Deserialize<CredentialsSentEvent>(userEvent);

        return new ActivityItemDto
        (
            Date: credentialsSentEvent!.SentDate,
            FullName: credentialsSentEvent.ExecutorFullName,
            Message: credentialsSentEvent.ToString()
        );
    }
}
