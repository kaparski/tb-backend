namespace TaxBeacon.UserManagement.Users.Activities.Models;

public abstract class UserEventBase
{
    protected UserEventBase(Guid executorId, string executorRoles, string executorFullName)
    {
        ExecutorRoles = executorRoles;
        ExecutorFullName = executorFullName;
        ExecutorId = executorId;
    }

    public string ExecutorRoles { get; }

    public string ExecutorFullName { get; }

    public Guid ExecutorId { get; }
}