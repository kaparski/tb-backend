using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public sealed class UserReactivatedEventFactory: IUserActivityFactory
    {
        public uint Revision => 1;

        public EventType EventType => EventType.UserReactivated;

        public UserActivityItemDto Create(string userEvent)
        {
            var userReactivatedEvent = JsonSerializer.Deserialize<UserReactivatedEvent>(userEvent);

            return new UserActivityItemDto
            (
                Date: userReactivatedEvent!.ReactivatedDate,
                FullName: userReactivatedEvent.ExecutorFullName,
                Message: userReactivatedEvent.ToString()
            );
        }
    }
}
