namespace TaxBeacon.Administration.Users.Activities.Models;

public class CredentialsSentEvent: UserEventBase
{
    public CredentialsSentEvent(Guid executorId, string createdUserEmail, DateTime sentDate, string executorFullName, string executorRoles)
        : base(executorId, executorRoles, executorFullName)
    {
        CreatedUserEmail = createdUserEmail;
        SentDate = sentDate;
    }

    public string CreatedUserEmail { get; }

    public DateTime SentDate { get; }

    public override string ToString()
        => "Credentials sent";
}
