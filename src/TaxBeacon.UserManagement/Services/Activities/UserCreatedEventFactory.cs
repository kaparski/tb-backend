using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public sealed class UserCreatedEventFactory: IUserActivityFactory
    {
        public uint Revision => 1;

        public EventType EventType => EventType.UserCreated;

        private readonly IDateTimeFormatter _dateTimeFormatter;

        public UserCreatedEventFactory(IDateTimeFormatter dateTimeFormatter) => _dateTimeFormatter = dateTimeFormatter;

        public UserActivityDto Create(string userEvent)
        {
            var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(userEvent);

            return new UserActivityDto
            {
                Date = _dateTimeFormatter.FormatDate(userCreatedEvent!.CreatedDate),
                FullName = userCreatedEvent.FullName,
                Message = userCreatedEvent.ToString(_dateTimeFormatter)
            };
        }
    }
}
