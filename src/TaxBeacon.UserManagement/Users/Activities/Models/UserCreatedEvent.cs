namespace TaxBeacon.UserManagement.Users.Activities.Models;

public sealed class UserCreatedEvent: UserEventBase
{
    public string CreatedUserEmail { get; }

    public DateTime CreatedDate { get; }

    public UserCreatedEvent(Guid executorId, string createdUserEmail, DateTime createdDate, string executorFullName, string executorRoles)
        : base(executorId, executorRoles, executorFullName)
    {
        CreatedUserEmail = createdUserEmail;
        CreatedDate = createdDate;
    }

    public override string ToString()
        => "User created";
}