namespace TaxBeacon.UserManagement.Models.Activities
{
    public sealed class UserCreatedEvent: UserEventBase
    {
        public Guid CreatedById { get; }

        public string CreatedUserEmail { get; }

        public DateTime CreatedDate { get; }

        public UserCreatedEvent(Guid createdById, string createdUserEmail, DateTime createdDate, string executorFullName, string executorRoles)
            : base(executorRoles, executorFullName)
        {
            CreatedById = createdById;
            CreatedUserEmail = createdUserEmail;
            CreatedDate = createdDate;
        }

        public override string ToString()
            => "User created";
    }
}
