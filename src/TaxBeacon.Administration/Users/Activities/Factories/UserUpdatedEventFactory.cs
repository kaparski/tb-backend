using System.Text.Json;
using TaxBeacon.Administration.Users.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Users.Activities.Factories;

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