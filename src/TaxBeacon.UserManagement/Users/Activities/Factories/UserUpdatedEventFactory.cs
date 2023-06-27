using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Users.Activities.Models;

namespace TaxBeacon.UserManagement.Users.Activities.Factories;

public sealed class UserUpdatedEventFactory: IUserActivityFactory
{
    public uint Revision => 1;

    public UserEventType UserEventType => UserEventType.UserUpdated;

    public ActivityItemDto Create(string userEvent)
    {
        var userUpdatedEvent = JsonSerializer.Deserialize<UserUpdatedEvent>(userEvent);

        return new ActivityItemDto
        (
            Date: userUpdatedEvent!.UpdatedDate,
            FullName: userUpdatedEvent.ExecutorFullName,
            Message: userUpdatedEvent.ToString()
        );
    }
}