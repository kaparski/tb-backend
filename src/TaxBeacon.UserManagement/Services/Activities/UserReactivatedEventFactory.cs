using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public sealed class UserReactivatedEventFactory: IUserActivityFactory
    {
        public uint Revision => 1;

        public EventType EventType => EventType.UserReactivated;

        private readonly IDateTimeFormatter _dateTimeFormatter;

        public UserReactivatedEventFactory(IDateTimeFormatter dateTimeFormatter) => _dateTimeFormatter = dateTimeFormatter;

        public UserActivityDto Create(string userEvent)
        {
            var userReactivatedEvent = JsonSerializer.Deserialize<UserReactivatedEvent>(userEvent);

            return new UserActivityDto
            {
                Date = _dateTimeFormatter.FormatDate(userReactivatedEvent!.ReactivatedDate),
                FullName = userReactivatedEvent.FullName,
                Message = userReactivatedEvent.ToString(_dateTimeFormatter)
            };
        }
    }
}
