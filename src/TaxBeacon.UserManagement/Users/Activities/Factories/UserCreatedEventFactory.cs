using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Users.Activities.Models;

namespace TaxBeacon.UserManagement.Users.Activities.Factories;

public sealed class UserCreatedEventFactory: IUserActivityFactory
{
    public uint Revision => 1;

    public UserEventType UserEventType => UserEventType.UserCreated;

    public ActivityItemDto Create(string userEvent)
    {
        var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(userEvent);

        return new ActivityItemDto
        (
            Date: userCreatedEvent!.CreatedDate,
            FullName: userCreatedEvent.ExecutorFullName,
            Message: userCreatedEvent.ToString()
        );
    }
}