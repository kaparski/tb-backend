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

        private readonly IDateTimeFormatter _dateTimeFormatter;

        public UserDeactivatedEventFactory(IDateTimeFormatter dateTimeFormatter) => _dateTimeFormatter = dateTimeFormatter;

        public UserActivityDto Create(string userEvent)
        {
            var userDeactivatedEvent = JsonSerializer.Deserialize<UserDeactivatedEvent>(userEvent);

            return new UserActivityDto
            {
                Date = _dateTimeFormatter.FormatDate(userDeactivatedEvent!.DectivatedDate),
                FullName = userDeactivatedEvent.FullName,
                Message = userDeactivatedEvent.ToString(_dateTimeFormatter)
            };
        }
    }
}
