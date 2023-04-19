namespace TaxBeacon.Common.Models;

public abstract class EventBase
{
    protected EventBase(Guid executorId, string executorRoles, string executorFullName)
    {
        ExecutorRoles = executorRoles;
        ExecutorFullName = executorFullName;
        ExecutorId = executorId;
    }

    public string ExecutorRoles { get; set; }

    public string ExecutorFullName { get; set; }

    public Guid ExecutorId { get; set; }
}
