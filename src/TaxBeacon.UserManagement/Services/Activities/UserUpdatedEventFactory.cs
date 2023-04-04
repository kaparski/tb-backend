using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public sealed class UserUpdatedEventFactory: IUserActivityFactory
    {
        public uint Revision => 1;

        public EventType EventType => EventType.UserUpdated;

        private readonly IDateTimeFormatter _dateTimeFormatter;

        public UserUpdatedEventFactory(IDateTimeFormatter dateTimeFormatter) => _dateTimeFormatter = dateTimeFormatter;

        public UserActivityDto Create(string userEvent)
        {
            var userUpdatedEvent = JsonSerializer.Deserialize<UserUpdatedEvent>(userEvent);

            return new UserActivityDto
            {
                Date = _dateTimeFormatter.FormatDate(userUpdatedEvent!.UpdatedDate),
                FullName = userUpdatedEvent.FullName,
                Message = userUpdatedEvent.ToString(_dateTimeFormatter)
            };
        }
    }
}
