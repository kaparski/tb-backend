using System.Text.Json;
using TaxBeacon.Administration.Users.Activities.Models;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Users.Activities.Factories;

public class UnassignRolesEventFactory: IUserActivityFactory
{
    public uint Revision => 1;

    public UserEventType UserEventType => UserEventType.UserRolesUnassign;

    public ActivityItemDto Create(string userEvent)
    {
        var unassignRolesEvent = JsonSerializer.Deserialize<UnassignRolesEvent>(userEvent);

        return new ActivityItemDto
        (
            Date: unassignRolesEvent!.UnassignDate,
            FullName: unassignRolesEvent.ExecutorFullName,
            Message: unassignRolesEvent.ToString()
        );
    }
}