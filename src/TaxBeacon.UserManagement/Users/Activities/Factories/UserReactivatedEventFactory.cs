using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Users.Activities.Models;

namespace TaxBeacon.UserManagement.Users.Activities.Factories;

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