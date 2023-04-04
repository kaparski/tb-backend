using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public sealed class AssignRolesEventFactory: IUserActivityFactory
    {
        public uint Revision => 1;

        public EventType EventType => EventType.UserRolesAssign;

        private readonly IDateTimeFormatter _dateTimeFormatter;

        public AssignRolesEventFactory(IDateTimeFormatter dateTimeFormatter) => _dateTimeFormatter = dateTimeFormatter;

        public UserActivityDto Create(string userEvent)
        {
            var assignRolesEventFactory = JsonSerializer.Deserialize<AssignRolesEvent>(userEvent);

            return new UserActivityDto
            {
                Date = _dateTimeFormatter.FormatDate(assignRolesEventFactory!.AssignDate),
                FullName = assignRolesEventFactory.FullName,
                Message = assignRolesEventFactory.ToString(_dateTimeFormatter)
            };
        }
    }
}
