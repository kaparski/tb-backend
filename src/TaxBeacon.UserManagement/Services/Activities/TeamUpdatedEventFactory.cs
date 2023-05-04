using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Models.Activities.Tenant;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities;

namespace TaxBeacon.UserManagement.Services.Activities
{
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
}
