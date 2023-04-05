using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public sealed class UserDeactivatedEventFactory: IUserActivityFactory
    {
        public uint Revision => 1;

        public EventType EventType => EventType.UserDeactivated;

        public UserActivityItemDto Create(string userEvent)
        {
            var userDeactivatedEvent = JsonSerializer.Deserialize<UserDeactivatedEvent>(userEvent);

            return new UserActivityItemDto
            (
                Date: userDeactivatedEvent!.DeactivatedDate,
                FullName: userDeactivatedEvent.ExecutorFullName,
                Message: userDeactivatedEvent.ToString()
            );
        }
    }
}
