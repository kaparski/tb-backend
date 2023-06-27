using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Users.Activities.Models;

namespace TaxBeacon.UserManagement.Users.Activities.Factories;

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
