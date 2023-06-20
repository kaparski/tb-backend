using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Models.Activities;

namespace TaxBeacon.UserManagement.Services.Activities
{
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
}
