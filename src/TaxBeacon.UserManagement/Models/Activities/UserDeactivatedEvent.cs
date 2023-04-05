namespace TaxBeacon.UserManagement.Models.Activities
{
    public sealed class UserDeactivatedEvent: UserEventBase
    {
        public Guid DeactivatedById { get; }

        public DateTime DeactivatedDate { get; }

        public UserDeactivatedEvent(Guid deactivatedById, DateTime deactivatedDate, string executorFullName, string executorRoles)
            : base(executorFullName, executorRoles)
        {
            DeactivatedById = deactivatedById;
            DeactivatedDate = deactivatedDate;

        }

        public override string ToString() => $"User deactivated";
    }
}
