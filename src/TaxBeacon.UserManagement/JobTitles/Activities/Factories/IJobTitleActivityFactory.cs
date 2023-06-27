using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.JobTitles.Activities.Factories;

public interface IJobTitleActivityFactory
{
    public uint Revision { get; }

    public JobTitleEventType EventType { get; }

    public ActivityItemDto Create(string activityEvent);
}
