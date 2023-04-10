using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public class UnassignRolesEventFactory: IUserActivityFactory
    {
        public uint Revision => 1;

        public EventType EventType => EventType.UserRolesUnassign;

        public UserActivityItemDto Create(string userEvent)
        {
            var unassignRolesEvent = JsonSerializer.Deserialize<UnassignRolesEvent>(userEvent);

            return new UserActivityItemDto
            (
                Date: unassignRolesEvent!.UnassignDate,
                FullName: unassignRolesEvent.ExecutorFullName,
                Message: unassignRolesEvent.ToString()
            );
        }
    }
}
