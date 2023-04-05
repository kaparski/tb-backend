using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public sealed class UserCreatedEventFactory: IUserActivityFactory
    {
        public uint Revision => 1;

        public EventType EventType => EventType.UserCreated;

        public UserActivityItemDto Create(string userEvent)
        {
            var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(userEvent);

            return new UserActivityItemDto
            (
                Date: userCreatedEvent!.CreatedDate,
                FullName: userCreatedEvent.FullName,
                Message: userCreatedEvent.ToString()
            );
        }
    }
}
