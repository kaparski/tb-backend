using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;
using TaxBeacon.UserManagement.JobTitles.Activities.Models;

namespace TaxBeacon.UserManagement.JobTitles.Activities.Factories;

public class JobTitleUpdatedEventFactory: IJobTitleActivityFactory
{
    public uint Revision => 1;

    public JobTitleEventType EventType => JobTitleEventType.JobTitleUpdatedEvent;

    public ActivityItemDto Create(string activityEvent)
    {
        var evt = JsonSerializer.Deserialize<JobTitleUpdatedEvent>(activityEvent);

        return new ActivityItemDto
        (
            Date: evt!.UpdatedDate,
            FullName: evt.ExecutorFullName,
            Message: evt.ToString()
        );
    }
}
