using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Users.Activities.Models;

namespace TaxBeacon.UserManagement.Users.Activities.Factories;

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