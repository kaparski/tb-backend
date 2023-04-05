namespace TaxBeacon.UserManagement.Models.Activities
{
    public sealed class UserReactivatedEvent: UserEventBase
    {
        public DateTime ReactivatedDate { get; }

        public UserReactivatedEvent(Guid executorId, DateTime reactivatedDate, string executorFullName, string executorRoles)
            : base(executorId, executorFullName, executorRoles) => ReactivatedDate = reactivatedDate;

        public override string ToString() => "User reactivated";
    }
}
