using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Models.Activities;

namespace TaxBeacon.UserManagement.Services.Activities
{
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
}
