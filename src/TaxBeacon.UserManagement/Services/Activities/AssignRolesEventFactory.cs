using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public sealed class AssignRolesEventFactory: IUserActivityFactory
    {
        public uint Revision => 1;

        public EventType EventType => EventType.UserRolesAssign;

        public UserActivityItemDto Create(string userEvent)
        {
            var assignRolesEventFactory = JsonSerializer.Deserialize<AssignRolesEvent>(userEvent);

            return new UserActivityItemDto
            (
                Date: assignRolesEventFactory!.AssignDate,
                FullName: assignRolesEventFactory.FullName,
                Message: assignRolesEventFactory.ToString()
            );
        }
    }
}
