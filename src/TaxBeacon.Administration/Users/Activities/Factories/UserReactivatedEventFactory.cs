using System.Text.Json;
using TaxBeacon.Administration.Users.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Users.Activities.Factories;

public sealed class UserReactivatedEventFactory: IUserActivityFactory
{
    public uint Revision => 1;

    public UserEventType UserEventType => UserEventType.UserReactivated;

    public ActivityItemDto Create(string userEvent)
    {
        var userReactivatedEvent = JsonSerializer.Deserialize<UserReactivatedEvent>(userEvent);

        return new ActivityItemDto
        (
            Date: userReactivatedEvent!.ReactivatedDate,
            FullName: userReactivatedEvent.ExecutorFullName,
            Message: userReactivatedEvent.ToString()
        );
    }
}