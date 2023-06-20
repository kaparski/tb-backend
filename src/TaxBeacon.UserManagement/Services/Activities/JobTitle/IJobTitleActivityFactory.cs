using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Services.Activities.JobTitle;

public interface IJobTitleActivityFactory
{
    public uint Revision { get; }

    public JobTitleEventType EventType { get; }

    public ActivityItemDto Create(string activityEvent);
}
