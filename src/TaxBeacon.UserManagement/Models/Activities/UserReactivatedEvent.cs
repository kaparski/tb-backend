namespace TaxBeacon.UserManagement.Models.Activities
{
    public sealed class UserReactivatedEvent: UserEventBase
    {
        public Guid ReactivatedById { get; }

        public DateTime ReactivatedDate { get; }

        public UserReactivatedEvent(Guid reactivatedById, DateTime reactivatedDate, string executorFullName, string executorRoles)
            : base(executorFullName, executorRoles)
        {
            ReactivatedById = reactivatedById;
            ReactivatedDate = reactivatedDate;
        }

        public override string ToString() => "User reactivated";
    }
}
