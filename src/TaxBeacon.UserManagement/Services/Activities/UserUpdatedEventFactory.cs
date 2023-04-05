using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public sealed class UserUpdatedEventFactory: IUserActivityFactory
    {
        public uint Revision => 1;

        public EventType EventType => EventType.UserUpdated;

        public UserActivityItemDto Create(string userEvent)
        {
            var userUpdatedEvent = JsonSerializer.Deserialize<UserUpdatedEvent>(userEvent);

            return new UserActivityItemDto
            (
                Date: userUpdatedEvent!.UpdatedDate,
                FullName: userUpdatedEvent.ExecutorFullName,
                Message: userUpdatedEvent.ToString()
            );
        }
    }
}
