using System.Text.Json;
using TaxBeacon.Administration.Users.Activities.Models;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Users.Activities.Factories;

public sealed class UserDeactivatedEventFactory: IUserActivityFactory
{
    public uint Revision => 1;

    public UserEventType UserEventType => UserEventType.UserDeactivated;

    public ActivityItemDto Create(string userEvent)
    {
        var userDeactivatedEvent = JsonSerializer.Deserialize<UserDeactivatedEvent>(userEvent);

        return new ActivityItemDto
        (
            Date: userDeactivatedEvent!.DeactivatedDate,
            FullName: userDeactivatedEvent.ExecutorFullName,
            Message: userDeactivatedEvent.ToString()
        );
    }
}