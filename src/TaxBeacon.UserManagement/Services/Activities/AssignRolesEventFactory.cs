using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services.Activities
{
    public sealed class AssignRolesEventFactory: IUserActivityFactory
    {
        public uint Revision => 1;

        public UserEventType UserEventType => UserEventType.UserRolesAssign;

        public ActivityItemDto Create(string userEvent)
        {
            var assignRolesEventFactory = JsonSerializer.Deserialize<AssignRolesEvent>(userEvent);

            return new ActivityItemDto
            (
                Date: assignRolesEventFactory!.AssignDate,
                FullName: assignRolesEventFactory.ExecutorFullName,
                Message: assignRolesEventFactory.ToString()
            );
        }
    }
}
