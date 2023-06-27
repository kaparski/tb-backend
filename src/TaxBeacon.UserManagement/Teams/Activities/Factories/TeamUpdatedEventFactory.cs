using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.Teams.Activities.Models;

namespace TaxBeacon.UserManagement.Teams.Activities.Factories;

public class TeamUpdatedEventFactory: ITeamActivityFactory
{
    public uint Revision => 1;

    public TeamEventType EventType => TeamEventType.TeamUpdatedEvent;

    public ActivityItemDto Create(string teamEvent)
    {
        var teamUpdatedEvent = JsonSerializer.Deserialize<TeamUpdatedEvent>(teamEvent);

        return new ActivityItemDto
        (
            Date: teamUpdatedEvent!.UpdatedDate,
            FullName: teamUpdatedEvent.ExecutorFullName,
            Message: teamUpdatedEvent.ToString()
        );
    }
}